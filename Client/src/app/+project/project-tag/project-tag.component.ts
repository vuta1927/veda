import { Component, OnInit, OnDestroy, ViewChildren, ViewEncapsulation, ViewContainerRef, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag, ExcluseArea, Coordinate, DataUpdate, TaggedTimeUser } from '../../shared/models/tag.model';
import { TagService } from '../services/tag.service';
import { QcService } from '../services/qc.service';
import { ClassList } from '../../shared/models/class.model';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../../shared/services/security.service';
import { Constants } from '../../constants';
import { TimeService } from '../services/timer.service';
import { HubConnection } from '@aspnet/signalr';
import { MessageTypes } from '../project-details/messageType';

import { Idle, DEFAULT_INTERRUPTSOURCES } from '@ng-idle/core';
import { Keepalive } from '@ng-idle/keepalive';
import context_menu from 'devextreme/ui/context_menu';
import { ProjectSettingService } from '../../+administrator/settings/settings.service';
import { ProjectUserSecurityService } from '../../shared/services/projectUserRole.service';
import { ProjectSetting } from '../../shared/models/project-setting.model';
import { QuantityCheckForView } from '../../shared/models/quantityCheck.model';
import swal from 'sweetalert2';
import { JSONP_ERR_WRONG_RESPONSE_TYPE } from '@angular/common/http/src/jsonp';

@Component({
    selector: 'app-project-tag',
    templateUrl: 'project-tag.component.html',
    styleUrls: ['project-tag.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class ProjecTagComponent {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    imageId: string = '';
    classData: ClassList[] = [];
    currentImage: any = {};
    apiUrl: string = '';
    canvas: any;
    tagMode: boolean = false;
    isPanning: boolean = false;
    selection: boolean = false;
    projectId: string = null;
    selectedTag: Tag;
    selectedObject: any;
    tags: Array<Tag> = new Array<Tag>();
    tagsForAddOrUpdate: Array<Tag> = new Array<Tag>();
    canvasTags: any = [];
    canvasLabels: any = [];
    imageWidth: number = 0;
    imageHeight: number = 0;
    imageUrl: string = '';
    btnSaveEnabled: boolean = true;
    excluseMode: boolean = false;
    polygonPoints: any = [];
    startPoint: any;
    lines: any = [];
    ExcluseAreas: ExcluseArea[] = [];
    excluseArea: ExcluseArea = new ExcluseArea();
    dataToUpdate: DataUpdate;
    currentUserId: number;
    messageTypes: MessageTypes = new MessageTypes();
    qcValues: any = [{ name: 'Passed', value: true }, { name: 'Unpassed', value: false }];
    qcValue: any = true;
    timedOut = false;
    lastPing?: Date = null;

    isAdmin: boolean = false;
    isProjectManager: boolean = false;
    isTeacher: boolean = false;
    isQc: boolean = false;

    qcComment: string = '';
    hadQc: boolean = false;
    tagsFromSrv: any = [];
    taggedTimeUsers: TaggedTimeUser[] = [];
    totalTaggedTime = 0;
    quantityCheckForViews: QuantityCheckForView[] = [];
    qcDone: boolean = false;
    selectedTags: any = [];
    projectSetting: ProjectSetting;
    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private imgService: ImageService,
        private classService: ClassService,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private configurationService: ConfigurationService,
        private tagSerivce: TagService,
        private authSevice: SecurityService,
        private timerSerive: TimeService,
        private idle: Idle,
        private keepalive: Keepalive,
        private securityServce: SecurityService,
        private qcService: QcService,
        private settingService: ProjectSettingService,
        private renderer: Renderer2,
        private projectUserService: ProjectUserSecurityService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        this.setUpIdleTimeout(300, 5); //value in second
    }

    ngOnInit() {
        this.setUpGlobalEvent();
        this.currentUserId = this.authSevice.getUserId();
        this.apiUrl = this.configurationService.serverSettings.apiUrl;
        let mother = this;
        Helpers.setLoading(true);
        this.route.queryParams.subscribe(params => {
            this.projectId = params.project;
            this.CheckRole();
            if (params.id) {
                this.imageId = params.id;
            } else {
                this.imageId = '0';
            }
            mother.settingService.getSetting(mother.projectId).toPromise().then(response => {
                mother.projectSetting = response.result;
                this.getImage();
            }).catch(response => {
                if (response.status == 401 || response.status == 403) {
                    mother.router.navigate(['#']);
                    return;
                }
                swal({
                    title: '', text: response.error ? response.error.message : response.message, type: 'error',
                    animation: false
                }).then(() => {
                    Helpers.setLoading(false);
                    mother.router.navigate(['project-details', { id: mother.projectId }]);
                })
            })
        });
    }

    CheckRole(): void {
        this.projectUserService.getRoles(this.projectId).toPromise().then(Response => {
            if (Response.result) {
                if (Response.result.roleName == Constants.ProjectManager) {
                    this.isProjectManager = true;
                } else if (Response.result.roleName == Constants.Teacher) {
                    this.isTeacher = true;
                } else if (Response.result.roleName == Constants.QuantityCheck) {
                    this.isQc = true;
                }
            }
        }).catch(Response => {
            if (Response.status == 401 || Response.status == 403) {
                this.router.navigate(['#']);
                return;
            }
        });

        this.isAdmin = this.securityServce.isInRole(Constants.admin);
        if (this.isAdmin) {
            this.isProjectManager = this.isTeacher = this.isQc = true;
        }
    }

    ngOnDestroy(): void {
        //Called once, before the instance is destroyed.
        //Add 'implements OnDestroy' to the class.
        // console.log("leaving tag page ...");
        if (this.idle) {
            this.idle.stop();
        }
        if (this.imageId)
            this.imgService.relaseImage(this.currentUserId, this.projectId, this.imageId).toPromise().then().catch(err => {

                if (err.status == 401 || err.status == 403) {
                    this.router.navigate(['#']);
                    return;
                }
                console.log(err.error ? err.error.text : err.message)
            });


    }

    setUpIdleTimeout(idleTime: number, timeOut: number) {
        // sets an idle timeout of idleTime seconds, for testing purposes.
        this.idle.setIdle(idleTime);
        // sets a timeout period of timeOut seconds. after (timeOut + idleTime) seconds of inactivity, the user will be considered timed out.
        this.idle.setTimeout(timeOut);
        // sets the default interrupts, in this case, things like clicks, scrolls, touches to the document
        this.idle.setInterrupts(DEFAULT_INTERRUPTSOURCES);
        let mother = this;
        // this.idle.onIdleEnd.subscribe(() => console.log('No longer idle.'));
        this.idle.onTimeout.subscribe(() => {
            this.timedOut = true;
            console.log('timeout!');
            mother.router.navigate(['project-details', { id: mother.projectId }]);
        });
        // this.idle.onIdleStart.subscribe(() => console.log('You\'ve gone idle!'));
        // this.idle.onTimeoutWarning.subscribe((countdown) => console.log('You will time out in ' + countdown + ' seconds!'));

        // sets the ping interval to 15 seconds
        this.keepalive.interval(10);

        this.keepalive.onPing.subscribe(() => {
            this.lastPing = new Date();
            this.imgService.sendPing(this.projectId, this.imageId).toPromise().then(Response => {

            }).catch(Response => {
                if (Response.status == 401 || Response.status == 403) {
                    mother.router.navigate(['#']);
                    return;
                }
            });

        });

        this.resetIdle();
    }

    resetIdle() {
        this.idle.watch();
        this.timedOut = false;
    }

    backImagePage() {
        this.router.navigate(['project-details', { id: this.projectId }]);
    }

    getImage() {
        let mother = this;
        this.classService.getClasses(this.projectId).toPromise().then(Response => {
            if (Response && Response.result) {
                mother.classData = [];
                Response.result.forEach(tag => {
                    mother.classData.push(new ClassList(tag.id, tag.name, false, tag.classColor));
                });
                // mother.generateClassContainer();
            }
        }).catch(error => {
            if (error.status == 401 || error.status == 403) {
                mother.router.navigate(['#']);
                return;
            }
            mother.showError("Cant get classes!");
        });
        if (this.imageId && this.imageId != '0') {
            this.imgService.getImageById(this.currentUserId, this.projectId, this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    mother.getTags(Response.result);
                }
            }).catch(err => {
                if (err.status == 401 || err.status == 403) {
                    mother.router.navigate(['#']);
                    return;
                }
                swal({ text: err.error ? err.error.text : err.message, type: 'error' }).then(() => {
                    mother.imageId = null;
                    mother.router.navigate(['project-details', { id: mother.projectId }])
                });
            });
        } else {
            this.imgService.getNextImage(this.currentUserId, this.projectId, this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    mother.getTags(Response.result);
                }
            }).catch(err => {
                if (err.status == 401 || err.status == 403) {
                    mother.router.navigate(['#']);
                    return;
                }
                swal({ text: err.error ? err.error.text : err.message, type: 'error' }).then(() => {
                    mother.imageId = null;
                    mother.router.navigate(['project-details', { id: mother.projectId }])
                });

            });
        }
    }

    getTags(image) {
        this.imageId = image.id;
        this.hadQc = image.quantityCheck ? true : false;
        this.currentImage = image;
        try {
            this.totalTaggedTime = image.userTaggedTimes ? image.userTaggedTimes.find(x => x.userId == this.currentUserId).taggedTime : 0;
        } catch (error) {
            this.totalTaggedTime = 0;
        }

        // console.log(this.totalTaggedTime);
        this.imageUrl = this.apiUrl + '/' + this.currentImage.path;

        this.getQc(image);
        this.tagSerivce.getTags(this.imageId).toPromise().then(Response => {
            if (Response && Response.result) {
                Helpers.setLoading(false);
                this.tags = Response.result;
                this.tagsFromSrv = Response.result;

                this.initCanvas();
            }
        });
    }

    getQc(image) {
        for (var i = 1; i <= this.projectSetting.quantityCheckLevel; i++) {
            // const level = 'value'+i;
            // const commentLevel = 'commentLevel' + i;
            // var tes = image.quantityCheck[level];
            if (image.quantityCheck && image.quantityCheck['value' + i] != null) {
                let newQc = new QuantityCheckForView();
                newQc.level = i;
                newQc.value = image.quantityCheck['value' + i];
                newQc.comment = image.quantityCheck['commentLevel' + i];
                newQc.href = '#qcLevel-' + i;
                newQc.collapseId = 'qcLevel-' + i;
                this.quantityCheckForViews.push(newQc);

                if (i == this.projectSetting.quantityCheckLevel) {
                    this.qcDone = true;
                }
            }
        }
    }

    GetNextImage() {
        this.resetVariables();
        let mother = this;
        this.classService.getClasses(this.projectId).toPromise().then(Response => {
            if (Response && Response.result) {
                mother.classData = [];
                Response.result.forEach(tag => {
                    mother.classData.push(new ClassList(tag.id, tag.name, false, tag.classColor));
                });
                // mother.generateClassContainer();
            }
        }).catch(error => {
            if (error.status == 401 || error.status == 403) {
                mother.router.navigate(['#']);
                return;
            }
            mother.showError("Cant get classes!");
        });
        this.imgService.getNextImage(this.currentUserId, this.projectId, this.imageId).toPromise().then(Response => {
            if (Response && Response.result) {
                mother.getTags(Response.result);
            }
        }).catch(err => {
            if (err.status == 401 || err.status == 403) {
                mother.router.navigate(['#']);
                return;
            }
            swal({ text: err.error ? err.error.text : err.message, type: 'error' }).then(() => {
                mother.imageId = null;
                mother.router.navigate(['project-details', { id: mother.projectId }])
            });

        });
    }

    resetVariables() {
        this.currentImage = {};
        this.classData = [];
        // this.tagMode = false;
        // this.isPanning = false;
        this.selection = false;
        this.selectedTag = null;
        this.tags = new Array<Tag>();
        this.tagsForAddOrUpdate = new Array<Tag>();
        this.canvasTags = [];
        this.canvasLabels = [];
        this.imageWidth = 0;
        this.imageHeight = 0;
        this.imageUrl = '';
        this.btnSaveEnabled = true;
        this.excluseMode = false;
        this.polygonPoints = [];
        this.startPoint = null;
        this.lines = [];
        this.ExcluseAreas = [];
        this.excluseArea = new ExcluseArea();
        this.dataToUpdate = null;
        this.quantityCheckForViews = [];
        this.qcComment = null;
        this.hadQc = false;
        this.qcDone = false;
    }

    switchTagMode() {
        if (this.tagMode) {
            this.tagMode = false;
            $(':focus').blur();
            this.updateTaggedTime().toPromise().catch(Response => {
                if (Response.status == 401 || Response.status == 403) {
                    this.router.navigate(['#']);
                    return;
                }
            });
        }
        else {
            this.tagMode = true;
            this.excluseMode = false;
            this.isPanning = false;
            this.timerSerive.startTimer();
        }
    }

    qcValueSelectChange(e) {
        var value = e.target.value;
        this.qcValue = value;
    }

    classRadioChange(e) {
        if (this.selectedTag == null) return;

        var value = Number(e.target.value);

        if (e.target.checked) {
            this.classData.forEach(c => {
                if ($(`#${c.id}`).val() != value && $(`#${c.id}`).is(':checked') === true) {
                    $(`#${c.id}`).prop('checked', false);
                };
            });
        }

        let tag;

        if (this.tagsForAddOrUpdate.indexOf(this.selectedTag) != -1) {
            tag = this.tagsForAddOrUpdate.find(x => x.index == this.selectedTag.index);
        } else {
            tag = this.tags.find(x => x.index == this.selectedTag.index);
        }

        tag.classId = 0;

        if (e.target.checked) {
            tag.classId = value;
        }


        let color: string;
        if (tag.classId > 0) {
            color = e.target.labels[0].childNodes[3].attributes[0].value.split(':')[1].split(';')[0].trim();
        } else {
            color = '#ccc';
        }

        this.canvasTags.forEach(cvTag => {
            if (cvTag.index == tag.index) {
                cvTag.set('stroke', color);
                cvTag.set('originColor', color);

            }
        });

        this.canvasLabels.forEach(cvLabel => {
            if (cvLabel.index == tag.index) {
                cvLabel.set('originColor', color);
                cvLabel.set('stroke', color);
                cvLabel.set('fill', color);

            }
        });

        this.canvas.renderAll();

        this.classData.forEach(c => {
            if (c.id == value) {
                if (e.target.checked)
                    c.checked = true;
                else
                    c.checked = false;

            }
        });
    }

    nextImage() {
        Helpers.setLoading(true);
        this.btnSaveEnabled = false;
        if (this.tagMode) {
            this.updateTaggedTime().toPromise().then(Response => {
                this.GetNextImage();
            }).catch(Response => {
                if (Response.status == 401 || Response.status == 403) {
                    this.router.navigate(['#']);
                    return;
                }
            });
            this.tagMode = false;
        } else {
            this.GetNextImage();
        }


    }

    saveChange() {
        if (this.tagMode) {
            if (this.timerSerive.timerStart) {
                this.totalTaggedTime += (this.timerSerive.getTotalTime() / 60);
                this.timerSerive.stop();
            }
        }

        this.tagMode = false;
        Helpers.setLoading(true);
        var mother = this;
        this.btnSaveEnabled = false;
        // console.log(this.currentImage.ignored);
        if (this.isTeacher || this.isProjectManager && !this.hadQc) {
            this.dataToUpdate = new DataUpdate(this.currentUserId, this.tagsForAddOrUpdate, this.totalTaggedTime, this.ExcluseAreas, this.currentImage.ignored);

            this.tagSerivce.saveTags(this.projectId, this.imageId, this.dataToUpdate).toPromise().then(Response => {
                if (Response) {
                    Helpers.setLoading(false);
                    mother.GetNextImage();
                }
            }).catch(errorResp => {
                if (errorResp.status == 401 || errorResp.status == 403) {
                    mother.router.navigate(['#']);
                    return;
                }
                Helpers.setLoading(false);
                swal({ text: errorResp.error ? errorResp.error.text : errorResp.message, type: 'error' });
                mother.btnSaveEnabled = true;
            });
        }


    }

    saveQcStage() {
        const mother = this;
        Helpers.setLoading(true);
        if (!this.tags || this.tags.length <= 0) {
            swal({
                text: 'Cannot set Quantity Check stage in image that not have tags !',
                animation: false
            }).then(() => {
                Helpers.setLoading(false);
            });
            return;
        }
        if (this.isQc || this.isProjectManager) {
            if (this.qcValue == 'false' && !this.qcComment) {
                swal({
                    title: 'Empty Comment', text: 'Can not set quantity check to Unpassed without comment, please enter comment!', type: 'error',
                    animation: false
                }).then(() => {
                    Helpers.setLoading(false);
                    document.getElementById('qcComment').focus();
                });
            } else {
                let data = { userId: this.currentUserId, imageId: this.imageId, qcValue: this.qcValue, qcComment: this.qcComment };
                this.qcService.saveQc(data).toPromise().then(Response => {
                    if (Response) {
                        Helpers.setLoading(false);
                        mother.GetNextImage();
                    }
                }).catch(err => {
                    if (err.status == 401 || err.status == 403) {
                        mother.router.navigate(['#']);
                        return;
                    }
                    swal({
                        title: '', text: err.error ? err.error.text : err.message, type: 'error',
                        animation: false
                    }).then(() => {
                        mother.btnSaveEnabled = true;
                        Helpers.setLoading(false);
                    })
                });
            }

        }
    }

    reload() {
        this.canvas.clear();
        this.tagSerivce.getTags(this.imageId).toPromise().then(Response => {
            if (Response && Response.result) {
                this.tags = Response.result;

                this.selection = false;
                this.selectedTag = null;
                this.canvasTags = [];
                this.canvasLabels = [];
                this.btnSaveEnabled = true;
                this.excluseMode = false;
                this.polygonPoints = [];
                this.startPoint = null;
                this.lines = [];
                this.ExcluseAreas = [];
                this.excluseArea = new ExcluseArea();
                this.dataToUpdate = null;
                this.setBackgroundImg();
            }
        });
        // this.tagMode = false;
        // this.isPanning = false;

    }

    getColorByClass(classId) {
        let element = document.getElementById(classId);
        var color = element['labels'][0].childNodes[3].attributes[0].value.split(':')[1].split(';')[0].trim();
        return color;
    }

    ExcluseAreaClick() {
        if (this.excluseMode) {
            this.excluseMode = false;
            $(':focus').blur();
            this.updateTaggedTime().toPromise().catch(Response => {
                if (Response.status == 401 || Response.status == 403) {
                    this.router.navigate(['#']);
                    return;
                }
            });
        }
        else {
            this.excluseMode = true;
            this.tagMode = false;
            this.isPanning = false;
            this.timerSerive.startTimer();
        }
    }

    updateTaggedTime() {
        if (this.timerSerive.timerStart) {
            this.totalTaggedTime += (this.timerSerive.getTotalTime() / 60);
            this.timerSerive.stop();
        }
        return this.imgService.updateTaggedTime(this.currentImage.id, this.totalTaggedTime);
    }

    initCanvas() {
        var mother = this;
        let container = document.getElementById('canvas-wrapper');
        container.innerHTML = `<canvas id="canvas" class="canvas"></canvas>`;

        this.canvas = new fabric.Canvas('canvas', { selection: false });
        fabric.Circle.prototype.originX = fabric.Circle.prototype.originY = 'center';
        fabric.Line.prototype.originX = fabric.Line.prototype.originY = 'center';

        this.setBackgroundImg();
        this.autoResizeCanvas();
        this.setupZoomFunc();
        this.setUpCanvasEvents();
    }


    setBackgroundImg() {
        let HideControls = {
            'tl': true,      //top left
            'tr': false,     //top right
            'bl': true,      //bottom left
            'br': true,      //bottom right
            'ml': true,      //middle left corner
            'mt': true,      //middle top corner
            'mr': true,      //middle right corner
            'mb': true,      //middle bottom corner
            'mtr': true,
        };
        let mother = this;
        fabric.Image.fromURL(this.apiUrl + '/' + this.currentImage.path, function (img) {
            img.selectable = false;
            img.setControlsVisibility(HideControls);
            img.set({
                name: 'background',
                left: 0,
                top: 0
            });
            var canvasHeight = document.getElementById("canvas").offsetHeight;
            var canvasWidth = document.getElementById("canvas").offsetWidth;

            // img.scaleToHeight(canvasHeight);
            // img.scaleToWidth(canvasWidth);

            mother.canvas.add(img);
            mother.canvas.sendToBack(img);
            mother.canvas.renderAll();

            mother.imageHeight = img.height;
            mother.imageWidth = img.width;

            mother.canvas.setZoom(canvasHeight / img.height);
            mother.drawTags();
        });
    }

    addDeleteBtn(x, y) {
        $("#deleteBtn").remove();
        var btnLeft = x + 1;
        var btnTop = y;
        var deleteBtn = '<img src="https://cdn2.iconfinder.com/data/icons/icojoy/shadow/standart/png/24x24/001_05.png" id="deleteBtn" style="position:absolute;top:' + btnTop + 'px;left:' + btnLeft + 'px;cursor:pointer;width:25px;height:25px;"/>';
        $("#canvas-wrapper").append(deleteBtn);
    }

    drawTags() {
        let mother = this;
        this.tags.forEach(tag => {
            let x = this.GetValueFromPercent(tag.left, mother.imageWidth);
            let y = this.GetValueFromPercent(tag.top, mother.imageHeight);
            let width = this.GetValueFromPercent(tag.width, mother.imageWidth);
            let height = this.GetValueFromPercent(tag.height, mother.imageHeight);
            let color = tag.classId > 0 ? mother.getColorByClass(tag.classId) : '#FFFFFF';
            let rect = new fabric.Rect({
                id: tag.id,
                index: tag.index,
                name: 'rect-' + tag.index,
                left: x,
                top: y,
                width: width,
                height: height,
                // width: width - x,
                // height: height - y,
                stroke: color,
                originColor: color,
                strokeWidth: 2,
                fill: '',
                transparentCorners: false,
                hasRotatingPoint: false,
                hasControls: this.isProjectManager || this.isTeacher,
                selectable: true
            });
            if (mother.hadQc) {
                rect.hasRotatingPoint = false;
                rect.hasControls = false;
            }

            let lock = false;
            if (this.isProjectManager) {
                if (this.hadQc) {
                    lock = true;
                }
            } else if (this.isQc && !this.isProjectManager) {
                lock = true;
            }
            rect.lockMovementX = lock;
            rect.lockMovementY = lock;
            rect.lockScalingX = lock;
            rect.lockScalingY = lock;
            rect.lockUniScaling = lock;
            rect.lockRotation = lock;


            this.bindingEvent(rect);
            this.canvas.add(rect);
            this.canvasTags.push(rect);

            let label = new fabric.IText(tag.index + '.', {
                name: 'lbl-' + tag.index,
                id: tag.id,
                index: tag.index,
                parentName: rect.get('name'),
                left: x,
                top: y - 30,
                fontFamily: "calibri",
                fontSize: 25,
                fill: color,
                stroke: color,
                originColor: color,
                strokeWidth: 0,
                hasRotatingPoint: false,
                centerTransform: true,
                selectable: false
            });
            this.canvas.add(label);
            this.canvasLabels.push(label);
        });
    }

    deleteObject() {
        var currentObject = this.selectedObject;
        var allObject = this.canvas.getObjects();

        if (currentObject.get('name').split('-')[0] == 'ex') {
            this.ExcluseAreas.splice(this.ExcluseAreas.indexOf(this.ExcluseAreas.find(x => x.name == currentObject.name)), 1);

            for (var i = 0; i < allObject.length; i++) {
                if (allObject[i].name == currentObject.name) {
                    this.canvas.remove(allObject[i]);
                }
            }
        } else {
            let tag = this.tags.find(x => x.index == currentObject.get('index'));
            if (tag) {
                this.tagSerivce.DeleteTag(tag.id).toPromise().then(Response => {
                    if (Response) {
                        if (this.tags.indexOf(this.selectedTag) != -1)
                            this.tags.splice(this.tags.indexOf(this.selectedTag), 1);

                        for (var i = 0; i < allObject.length; i++) {
                            if (allObject[i].index == currentObject.index) {
                                this.canvas.remove(allObject[i]);
                            }
                        }
                        for (var i = 0; i < allObject.length; i++) {
                            if (allObject[i].index == currentObject.index) {
                                this.canvas.remove(allObject[i]);
                            }
                        }
                    }
                }).catch(err => {
                    if (err.status == 401 || err.status == 403) {
                        this.router.navigate(['#']);
                        return;
                    }
                    console.log(err.error.text)
                });
            } else {
                tag = this.tagsForAddOrUpdate.find(x => x.index == currentObject.get('index'));
                this.tagsForAddOrUpdate.splice(this.tagsForAddOrUpdate.indexOf(tag), 1);

                for (var i = 0; i < allObject.length; i++) {
                    if (allObject[i].index == currentObject.index) {
                        this.canvas.remove(allObject[i]);
                    }
                }
                for (var i = 0; i < allObject.length; i++) {
                    if (allObject[i].index == currentObject.index) {
                        this.canvas.remove(allObject[i]);
                    }
                }
            }
        }
    }

    setUpGlobalEvent(){
        var mother = this;
        $(window).on("keyup", function(evt){
            if (evt.which === 13 && mother.excluseMode) {
                mother.finalize();
            };

            if (evt.which === 84) {
                if (mother.tagMode) {
                    mother.tagMode = false;
                    $(':focus').blur();
                    mother.updateTaggedTime().toPromise().catch(Response => {
                        if (Response.status == 401 || Response.status == 403) {
                            mother.router.navigate(['#']);
                            return;
                        }
                    });
                }
                else {
                    mother.tagMode = true;
                    mother.excluseMode = false;
                    mother.isPanning = false;
                    mother.timerSerive.startTimer();
                }
            }

            if (evt.which === 69) {
                if (mother.excluseMode) {
                    mother.excluseMode = false;
                    $(':focus').blur();
                    mother.updateTaggedTime().toPromise().catch(Response => {
                        if (Response.status == 401 || Response.status == 403) {
                            mother.router.navigate(['#']);
                            return;
                        }
                    });
                }
                else {
                    mother.excluseMode = true;
                    mother.tagMode = false;
                    mother.isPanning = false;
                    mother.timerSerive.startTimer();
                }
            }
        })

        $(window).on("keydown", function (evt) {
            if (evt.which === 46) {
                mother.deleteObject();
            }
        });
    }

    setUpCanvasEvents() {
        let isDown: boolean = false;
        let startPosition: any = {};
        let rect: any = {};
        let mother = this;
        $('body').on('contextmenu', 'canvas', function (event: any) {
            event.preventDefault();

        });

        fabric.util.addListener(window, "dblclick", function () {
            if (mother.excluseMode) {
                mother.finalize();
            }
        });

        this.canvas.on('mouse:down', function (event) {
            // console.log(event.button);
            var obj = event.target;

            if (obj && obj.get('type') != 'image') {
                var tag = mother.tagsForAddOrUpdate.find(x => x.index == obj.get('index'));
                if (!tag)
                    tag = mother.tags.find(x => x.index == obj.get('index'));

                mother.selectedTag = tag;
                mother.selectedObject = obj;
            } else {
                mother.selectedTag = null;
                mother.selectedObject = null;
            }
            // --- set up panning func ---
            var evt = event.e;
            if ( !mother.tagMode && !mother.excluseMode) { //evt.ctrlKey === true ||
                mother.isPanning = true;
                return;
            }
            //--- end pan func ---


            if (mother.excluseMode) {
                var _mouse = this.getPointer(event.e);
                var _x = _mouse.x;
                var _y = _mouse.y;
                if (mother.startPoint == null) {
                    var startPoint = new fabric.Circle({
                        left: _x,
                        top: _y,
                        fill: "#FFFFFF",
                        radius: 6,
                        strokeWidth: 1,
                        stroke: "black",
                        hoverCursor: "pointer",
                        selectable: false
                    });
                    mother.startPoint = startPoint;
                    startPoint.on('click', function () {
                        console.log('start point clicked');
                    });
                    this.add(startPoint);
                } else {
                    if (event.target == mother.startPoint) {
                        mother.finalize();
                        return;
                    }
                }

                var line = new fabric.Line([_x, _y, _x, _y], {
                    strokeWidth: 2,
                    selectable: false,
                    stroke: '#FFFFFF',
                    strokeDashArray: [5, 5],
                });
                mother.excluseArea.paths.push(new Coordinate(_x, _y));
                mother.polygonPoints.push(new fabric.Point(_x, _y));
                mother.lines.push(line);

                this.add(line);
                return;
            }
            //--- set up tag func ---
            if (mother.tagMode && (mother.isProjectManager || mother.isTeacher || !mother.hadQc)) {
                isDown = true;
                var pointer = this.getPointer(evt);
                startPosition.x = pointer.x;
                startPosition.y = pointer.y;

                let index = mother.generateId();

                rect = new fabric.Rect({
                    id: -1,
                    index: index,
                    name: 'rect-' + index,
                    left: pointer.x,
                    top: pointer.y,
                    width: 0,
                    height: 0,
                    stroke: '#FFFFFF',
                    originColor: '#FFFFFF',
                    strokeWidth: 2,
                    fill: '',
                    transparentCorners: false,
                    hasRotatingPoint: mother.isTeacher || mother.isProjectManager,
                    hasControls: mother.isTeacher || mother.isProjectManager,
                    selectable: true
                });
                this.add(rect);
            } 
        });

        this.canvas.on('mouse:move', function (event) {
            if (mother.lines.length && mother.excluseMode) {
                var _mouse = this.getPointer(event.e);
                mother.lines[mother.lines.length - 1].set({
                    x2: _mouse.x,
                    y2: _mouse.y
                }).setCoords();
                this.renderAll();
            }
            if (mother.isPanning) {
                var delta = new fabric.Point(event.e.movementX, event.e.movementY);
                this.relativePan(delta);
                return;
            }

            if (!isDown || mother.hadQc || (mother.isQc && !mother.isProjectManager)) return;

            if (mother.isTeacher || mother.isProjectManager) {
                var pointer = mother.canvas.getPointer(event.e);
                rect.set({ 'width': Math.abs(pointer.x - startPosition.x), 'height': Math.abs(pointer.y - startPosition.y) });
                mother.bindingEvent(rect);
                mother.canvas.renderAll();
            }


        });

        this.canvas.on('mouse:up', function () {
            mother.isPanning = false;
            isDown = false;
            if (!mother.tagMode) return;

            if (mother.hadQc) return;
            mother.bindingEvent(rect);
            mother.canvas.remove(rect);
            mother.canvas.add(rect);
            mother.canvasTags.push(rect);

            let index = rect.get('index');
            let id = rect.get('id');
            mother.tagsForAddOrUpdate.push(new Tag(
                -1,
                index,
                mother.imageId,
                0,
                0,
                mother.GetPercent(rect.get("top"), mother.imageHeight),
                mother.GetPercent(rect.get("left"), mother.imageWidth),
                mother.GetPercent(rect.get('width'), mother.imageWidth),
                mother.GetPercent(rect.get('height'), mother.imageHeight)
                // mother.GetPercent(rect.get("left") + (rect.get('width')), mother.imageWidth),
                // mother.GetPercent(rect.get("top") + (rect.get('height')), mother.imageHeight)
            ));
            let label = new fabric.IText(index + '.', {
                name: 'lbl-' + index,
                id: id,
                index: index,
                parentName: rect.get('name'),
                left: rect.get('left'),
                top: rect.get('top') - 30,
                fontFamily: "calibri",
                fontSize: 25,
                fill: '#FFFFFF',
                stroke: '#FFFFFF',
                originColor: '#FFFFFF',
                strokeWidth: 0,
                hasRotatingPoint: false,
                centerTransform: true,
                selectable: false
            });
            mother.canvas.add(label);
            mother.canvasLabels.push(label);
        });

        this.canvas.on('object:selected', function (e) {
            mother.tagMode = false;
            mother.isPanning = false;
            // $("#deleteBtn").remove();
            if ((mother.isQc && !mother.isProjectManager) || mother.hadQc) return;
        });

        this.canvas.on('object:modified', function (e) {
            mother.isPanning = false;
            if ((mother.isQc && !mother.isProjectManager) || mother.hadQc) return;
            // mother.addDeleteBtn(e.target.oCoords.tr.x, e.target.oCoords.tr.y);
        });

        this.canvas.on('object:scaling', function (e) {
            mother.isPanning = false;
            if ((mother.isQc && !mother.isProjectManager) || mother.hadQc) return;
        });

        this.canvas.on('object:rotating', function (e) {
            mother.isPanning = false;
            if ((mother.isQc && !mother.isProjectManager) || mother.hadQc) return;
        });

        this.canvas.on('selection:cleared', function () {
            mother.isPanning = false;
            if (!mother.tagMode || mother.isQc || mother.hadQc) return;
            mother.tagMode = true;
        });

        this.canvas.on('object:moving', function (e) {
            mother.isPanning = false;
            var target = e.target;
            // var scaleValue = target.scaleX;
            var pointer = mother.canvas.getPointer(e.e);
            var allObject = mother.canvas.getObjects();
            if (target.name.split('-')[0] == 'rect') {
                allObject.forEach(obj => {

                    var names = obj.get('name');
                    var parent = obj.get('parentName');

                    if (names.split('-')[0] == 'lbl' && parent == target.name) {
                        obj.set('left', target.get('left'));
                        obj.set('top', target.get('top') - 30);
                    }
                });
            }
        });

        this.canvas.on('object:rotating', function (e) {
            mother.isPanning = false;
            var target = e.target;
            var angle = target.get('angle');
            var pointer = mother.canvas.getPointer(e.e);
            var allObject = mother.canvas.getObjects();
            if (target.name.split('-')[0] == 'rect') {
                allObject.forEach(obj => {
                    var names = obj.get('name');
                    var parent = obj.get('parentName');

                    if (names.split('-')[0] == 'lbl' && parent == target.name) {
                        obj.set('angle', angle);
                        obj.set('left', target.get('left'));
                        obj.set('top', target.get('top') - 30);
                    }
                });
            }
        });
    }

    finalize() {
        this.excluseMode = false;
        let mother = this;
        this.lines.forEach(function (line) {
            mother.canvas.remove(line);
        });
        this.canvas.remove(this.startPoint);
        this.startPoint = null;
        this.canvas.add(mother.makePolygon()).renderAll();
        this.lines.length = 0;
        this.polygonPoints.length = 0;
    }

    makePolygon() {

        var left = fabric.util.array.min(this.polygonPoints, "x");
        var top = fabric.util.array.min(this.polygonPoints, "y");
        let mother = this;
        let name = 'ex-' + mother.generateId();
        this.polygonPoints.push(new fabric.Point(this.polygonPoints[0].x, this.polygonPoints[0].y));
        this.excluseArea.paths.push(new Coordinate(this.polygonPoints[0].x, this.polygonPoints[0].y));
        this.excluseArea.name = name;
        this.ExcluseAreas.push(this.excluseArea);
        this.excluseArea = new ExcluseArea();
        return new fabric.Polygon(this.polygonPoints.slice(), {
            left: left,
            top: top,
            fill: '#000000',
            stroke: 'black',
            name: name,
            id: -1,
            transparentCorners: false
        });
    }

    bindingEvent(rect: any) {
        var mother = this;

        rect.on({
            'selected': function (event) {
                mother.onTagSelectedEvent(this);
            },
            'moving': function (event) {
                mother.getNewCoodirnate(this);
            },
            'rotating': function (event) {
                mother.getNewCoodirnate(this);
            },
            'scaling': function (event) {
                mother.getNewCoodirnate(this);
            }
        });
    }

    getNewCoodirnate(target) {
        var activeObject = this.canvas.getActiveObject();
        if (activeObject.get('name') != target.name) {
            return;
        }

        let top = activeObject.get('top');
        let left = activeObject.get('left');
        let width = activeObject.get('width') * activeObject.scaleX;
        let height = activeObject.get('height') * activeObject.scaleY;

        let tag = this.tagsForAddOrUpdate.find(x => x.index == target.index);

        if (!tag) {
            tag = this.tags.find(x => x.index == target.index);
        }

        tag.top = this.GetPercent(top, this.imageHeight);
        tag.left = this.GetPercent(left, this.imageWidth);
        // tag.width = this.GetPercent((left + width), this.imageWidth);
        // tag.height = this.GetPercent((top + height), this.imageHeight);
        tag.width = this.GetPercent(width, this.imageWidth);
        tag.height = this.GetPercent(height, this.imageHeight);

        if (!this.tagsForAddOrUpdate.find(x => x.index == target.index)) {
            this.tagsForAddOrUpdate.push(tag);
        }

        // this.tagsForAddOrUpdate.forEach(t => {
        //     if (t.index == target.index) {
        //         t.top = this.GetPercent(top, this.imageHeight);
        //         t.left = this.GetPercent(left, this.imageWidth);
        //         t.width = this.GetPercent((left + width), this.imageWidth);
        //         t.height = this.GetPercent((top + height), this.imageHeight);
        //     };
        // });
    }

    onTagSelectedEvent(target) {
        let tag = this.tagsForAddOrUpdate.find(x => x.index == target.index);
        if (!tag) {
            tag = this.tags.find(x => x.index == target.index);
            this.tagsForAddOrUpdate.push(tag);
        }

        this.selectedTag = tag;

        for (let i = 0; i < this.classData.length; i++) {
            if (this.selectedTag.classId == this.classData[i].id) {
                this.classData[i].checked = true;
            } else {
                this.classData[i].checked = false;
            }
        }

        // this.tags.forEach(t => {
        //     if (t.index == target.index) {
        //         for (let i = 0; i < this.classData.length; i++) {
        //             if (t.classIds.indexOf(this.classData[i].id) != -1) {
        //                 this.classData[i].checked = true;
        //             } else {
        //                 this.classData[i].checked = false;
        //             }
        //         }
        //     }
        // })

    }

    autoResizeCanvas() {
        if (this.canvas.width != $('#canvas-wrapper').width()) {
            var scaleMultiplier = $('#canvas-wrapper').width() / this.canvas.width;
            var objects = this.canvas.getObjects();

            this.canvas.setWidth(this.canvas.getWidth() * scaleMultiplier);
            this.canvas.setHeight(this.canvas.getHeight() * scaleMultiplier);
            this.canvas.renderAll();
            this.canvas.calcOffset();
        }
        var mother = this;
        // $(window).resize(function () {
        //     if (mother.canvas.width != $("#canvas-wrapper").width()) {
        //         var scaleMultiplier = $("#canvas-wrapper").width() / mother.canvas.width;
        //         var objects = mother.canvas.getObjects();
        //         for (var i in objects) {
        //             objects[i].scaleX = objects[i].scaleX * scaleMultiplier;
        //             objects[i].scaleY = objects[i].scaleY * scaleMultiplier;
        //             objects[i].left = objects[i].left * scaleMultiplier;
        //             objects[i].top = objects[i].top * scaleMultiplier;
        //             objects[i].setCoords();
        //         }

        //         mother.canvas.setWidth(mother.canvas.getWidth() * scaleMultiplier);
        //         mother.canvas.setHeight(mother.canvas.getHeight() * scaleMultiplier);
        //         mother.canvas.renderAll();
        //         mother.canvas.calcOffset();
        //     }
        // });
        $("#canvas-wrapper").resize(function () {
            if (mother.canvas.width != $("#canvas-wrapper").width()) {
                var scaleMultiplier = $("#canvas-wrapper").width() / mother.canvas.width;
                var objects = mother.canvas.getObjects();
                for (var i in objects) {
                    objects[i].scaleX = objects[i].scaleX * scaleMultiplier;
                    objects[i].scaleY = objects[i].scaleY * scaleMultiplier;
                    objects[i].left = objects[i].left * scaleMultiplier;
                    objects[i].top = objects[i].top * scaleMultiplier;
                    objects[i].setCoords();
                }

                mother.canvas.setWidth(mother.canvas.getWidth() * scaleMultiplier);
                mother.canvas.setHeight(mother.canvas.getHeight() * scaleMultiplier);
                mother.canvas.renderAll();
                mother.canvas.calcOffset();
            }
        });
    }

    setupZoomFunc() {
        var mother = this;
        $(this.canvas.wrapperEl).on('mousewheel', function (e) {
            var delta = e.originalEvent['wheelDelta'] / 500;
            var pointer = mother.canvas.getPointer(e['e']);
            var scaleFactor = 0;
            var objects = mother.canvas.getObjects();
            if (delta > 0) {
                scaleFactor = mother.canvas.getZoom() * 1.1;
                mother.canvas.zoomToPoint(new fabric.Point(e.offsetX, e.offsetY), scaleFactor);
            }
            if (delta < 0) {
                scaleFactor = mother.canvas.getZoom() / 1.1;
                mother.canvas.zoomToPoint(new fabric.Point(e.offsetX, e.offsetY), scaleFactor);
            }
            return false;
        });
    }

    generateId() {
        let index: number = 1;
        return (index + this.getHighestIndex());
    }

    getHighestIndex() {
        // console.log(this.tags);
        let index: number = 0;
        this.tags.forEach(tag => {
            if (tag.index > index) {
                index = tag.index;
            }
        });
        this.tagsForAddOrUpdate.forEach(tag => {
            if (tag.index > index) {
                index = tag.index;
            }
        });

        return index;
    }

    showSuccess(message: string) {
        this.toastr.success(message, 'Success!', { toastLife: 2000, showCloseButton: true });
    }

    showError(message: string) {
        this.toastr.error(message, 'Oops!', { toastLife: 3000, showCloseButton: true });
    }

    showInfo(message: string) {
        this.toastr.info(message, null, { toastLife: 3000, showCloseButton: true });
    }

    GetPercent(value: number, total: number) {
        return (value / total);
    }

    GetValueFromPercent(value: number, total: number) {
        return (value * total);
    }
}