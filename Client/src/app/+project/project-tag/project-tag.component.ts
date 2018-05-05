import { Component, OnInit, OnDestroy, ViewChildren, ViewEncapsulation, ViewContainerRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag, ExcluseArea, Coordinate, DataUpdate } from '../../shared/models/tag.model';
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
    isDragging: boolean = false;
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
    tagBtnClass: string = 'btn btn-outline-primary btn-sm m-btn m-btn--icon m-btn--wide';
    excluseBtnClass: string = 'btn btn-outline-danger btn-sm m-btn m-btn--icon m-btn--wide';
    dataToUpdate: DataUpdate;
    currentUserId: number;
    messageTypes: MessageTypes = new MessageTypes();
    qcValues: any = [{ name: 'Passed', value: true }, { name: 'Falsed', value: false }];
    qcValue: boolean = false;
    timedOut = false;
    lastPing?: Date = null;

    isAdmin: boolean = false;
    viewTag: boolean = false; editTag: boolean = false; deleteTag: boolean = false; addTag: boolean = false; viewImg: boolean = false;
    viewQc: boolean = false; addQc: boolean = false; editQc: boolean = false; deleteQc: boolean = false;
    qcComment: string = '';
    isQc: boolean = false;
    tagsFromSrv: any = [];
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
        private qcService: QcService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        this.setUpIdleTimeout(600, 5); //value in second
    }

    ngOnInit() {
        this.isAdmin = this.securityServce.IsGranted(Constants.admin);
        this.viewTag = this.securityServce.IsGranted(Constants.ViewTag);
        this.editTag = this.securityServce.IsGranted(Constants.EditTag);
        this.deleteTag = this.securityServce.IsGranted(Constants.DeleteTag);
        this.addTag = this.securityServce.IsGranted(Constants.AddTag);
        this.viewImg = this.securityServce.IsGranted(Constants.viewImage);
        this.addQc = this.securityServce.IsGranted(Constants.QcAdd);
        this.editQc = this.securityServce.IsGranted(Constants.QcEdit);
        this.viewQc = this.securityServce.IsGranted(Constants.QcView);
        this.deleteQc = this.securityServce.IsGranted(Constants.QcDelete);


        this.currentUserId = this.authSevice.getUserId();
        this.apiUrl = this.configurationService.serverSettings.apiUrl;
        let mother = this;
        this.route.queryParams.subscribe(params => {
            this.projectId = params.project;
            if (params.id) {
                this.imageId = params.id;
                this.getImage(true);
            } else {
                this.imageId = '0';
                this.getImage();
            }
        });
    }

    ngOnDestroy(): void {
        //Called once, before the instance is destroyed.
        //Add 'implements OnDestroy' to the class.
        // console.log("leaving tag page ...");
        if (this.idle) {
            this.idle.stop();
        }
        if (this.imageId) {
            this.imgService.relaseImage(this.projectId, this.imageId).toPromise().then().catch(err => console.log(err.error.text));
        }

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
            mother.router.navigate(['project-details', { id: mother.projectId }]);
        });
        // this.idle.onIdleStart.subscribe(() => console.log('You\'ve gone idle!'));
        // this.idle.onTimeoutWarning.subscribe((countdown) => console.log('You will time out in ' + countdown + ' seconds!'));

        // sets the ping interval to 15 seconds
        this.keepalive.interval(5);

        this.keepalive.onPing.subscribe(() => {
            this.lastPing = new Date();
            this.imgService.sendPing(this.projectId, this.imageId).toPromise().then(Response => {

            }).catch(err => { mother.router.navigate(['project-details', { id: mother.projectId }]) });
        });

        this.resetIdle();
    }

    resetIdle() {
        this.idle.watch();
        this.timedOut = false;
    }

    setupHub() {
        // this._hubConnection = new HubConnection(this.apiUrl + '/project');
        // this._hubConnection
        //     .start()
        //     .then(() => console.log('connection started!'))
        //     .catch(err => console.log('Error while establishing connection !'));
        // this._hubConnection.on("send", data => { console.log(data) });
    }

    getImage(paramIdExist: boolean = false) {
        let mother = this;
        this.classService.getClasses(this.projectId).toPromise().then(Response => {
            if (Response && Response.result) {
                Response.result.forEach(tag => {
                    mother.classData.push(new ClassList(tag.id, tag.name, false, tag.classColor));
                });
                // mother.generateClassContainer();
            }
        }).catch(error => {
            mother.showError(error.error.text);
        });
        if (paramIdExist) {
            this.imgService.getImageById(this.currentUserId, this.projectId, this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    mother.getTags(Response.result);
                }
            }).catch(err => mother.router.navigate(['project-details', { id: mother.projectId }]));
        } else {
            this.imgService.getNextImage(this.currentUserId, this.projectId, this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    mother.getTags(Response.result);
                }
            }).catch(err => mother.router.navigate(['project-details', { id: mother.projectId }]));
        }
    }

    getTags(image) {
        this.imageId = image.id;
        this.isQc = image.quantityCheck ? true : false;
        this.qcValue = image.qcStatus;
        if (image.quantityCheck)
            this.qcComment = image.quantityCheck.comment ? image.quantityCheck.comment : '';
        this.currentImage = image;
        this.imageUrl = this.apiUrl + '/' + this.currentImage.path;

        this.tagSerivce.getTags(this.imageId).toPromise().then(Response => {
            if (Response && Response.result) {
                Helpers.setLoading(false);
                this.tags = Response.result;
                this.tagsFromSrv = Response.result;

                this.timerSerive.startTimer(this.currentImage.tagTime);
                this.initCanvas();
            }
        });
    }

    GetNextImage() {
        this.resetVariables();
        this.getImage();
    }

    resetVariables() {
        this.canvas.clear();
        this.currentImage = {};
        this.classData = [];
        // this.tagMode = false;
        // this.isDragging = false;
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
    }

    switchTagMode() {
        let origin = 'btn btn-outline-primary btn-sm m-btn m-btn--icon m-btn--wide';
        if (this.tagMode) {
            this.tagBtnClass = origin;
            this.tagMode = false;
        }
        else {
            this.tagBtnClass += ' active'
            this.tagMode = true;
            this.excluseMode = false;
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
        this.GetNextImage();

    }

    saveChange() {
        Helpers.setLoading(true);
        var mother = this;
        this.btnSaveEnabled = false;
        if (this.addTag && this.editTag && !this.isQc) {
            // this.tagsForAddOrUpdate.forEach(t => {
            //     t.top = this.GetPercent(t.top, this.imageHeight);
            //     t.left = this.GetPercent(t.left, this.imageWidth);
            //     t.width = this.GetPercent(t.width, this.imageWidth);
            //     t.height = this.GetPercent(t.height, this.imageHeight);
            // });

            this.dataToUpdate = new DataUpdate(this.currentUserId, this.tagsForAddOrUpdate, this.ExcluseAreas);

            this.tagSerivce.saveTags(this.projectId, this.imageId, this.dataToUpdate).toPromise().then(Response => {
                if (Response) {
                    Helpers.setLoading(false);
                    mother.GetNextImage();
                }
            }).catch(errorResp => {
                Helpers.setLoading(false);
                mother.showError(errorResp.error.text);
                mother.btnSaveEnabled = true;
            });
        } else if (this.addQc || this.editQc) {
            let data = { userId: this.currentUserId, imageId: this.imageId, qcValue: this.qcValue, qcComment: this.qcComment };
            this.qcService.saveQc(data).toPromise().then(Response => {
                if (Response) {
                    Helpers.setLoading(false);
                    mother.GetNextImage();
                }
            }).catch(err => { console.log(err.error.text) });
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
        // this.isDragging = false;

    }

    getColorByClass(classId) {
        let element = document.getElementById(classId);
        var color = element['labels'][0].childNodes[3].attributes[0].value.split(':')[1].split(';')[0].trim();
        return color;
    }

    ExcluseAreaClick() {
        if (this.excluseMode) {
            this.excluseMode = false;
        }
        else {
            this.excluseMode = true;
            this.tagMode = false;
        }
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
        this.setUpMouseEvent();
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

            mother.canvas.setZoom(canvasHeight/img.height);
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
                width: width - x,
                height: height - y,
                stroke: color,
                originColor: color,
                strokeWidth: 2,
                fill: '',
                transparentCorners: false,
                hasRotatingPoint: mother.addTag,
                hasControls: mother.addTag,
                selectable: true
            });
            if (mother.isQc) {
                rect.hasRotatingPoint = false;
                rect.hasControls = false;
            }
            rect.lockMovementX = !mother.addTag;
            rect.lockMovementY = !mother.addTag;
            rect.lockScalingX = !mother.addTag;
            rect.lockScalingY = !mother.addTag;
            rect.lockUniScaling = !mother.addTag;
            rect.lockRotation = !mother.addTag;


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
                }).catch(err => { console.log(err.error.text) });
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

    setUpMouseEvent() {
        let isDown: boolean = false;
        let startPosition: any = {};
        let rect: any = {};
        let mother = this;
        let isDragging: boolean = false;
        $('body').on('contextmenu', 'canvas', function (options: any) {
            //Disabe contextmenu on right mouse, implement right mouse event
            // let target: any = mother.canvas.findTarget(options, false);
            // if (target) {
            //     let type: string = target.type;
            //     if (type === "group") {
            //         console.log('right click on group');
            //     } else {
            //         // mother.canvas.setActiveObject(target);
            //         console.log('right click on target, type: ' + type);
            //     }
            // } else {
            //     // mother.canvas.discardActiveObject();
            //     // mother.canvas.discardActiveGroup();
            //     // mother.canvas.renderAll();
            //     console.log('right click on canvas');
            // }
            // isDragging = true;
            options.preventDefault();
            return false;
        });
        fabric.util.addListener(window, "dblclick", function () {
            if (mother.excluseMode) {
                mother.finalize();
            }
        });

        //
        fabric.util.addListener(window, "keyup", function (evt) {
            if (evt.which === 13 && mother.excluseMode) {
                mother.finalize();
            }
        });

        fabric.util.addListener(window, "keydown", function (evt) {
            if (evt.which === 46) {
                mother.deleteObject();
            }
        });

        this.canvas.on('mouse:down', function (event) {
            var obj = event.target;

            if (obj.get('type') != 'image') {
                var tag = mother.tagsForAddOrUpdate.find(x => x.index == obj.get('index'));
                if (!tag)
                    tag = mother.tags.find(x => x.index == obj.get('index'));

                mother.selectedTag = tag;
                mother.selectedObject = obj;
            }
            // --- set up panning func ---
            var evt = event.e;
            if (evt.altKey === true) {
                isDragging = true;
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
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
            }


            //--- set up tag func ---
            if (!mother.tagMode || isDragging || !mother.addTag || !mother.editTag || this.isQc) return;
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
                hasRotatingPoint: mother.addTag,
                hasControls: mother.addTag,
                selectable: true
            });
            this.add(rect);
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
            if (isDragging) {
                var evt = event.e;
                this.viewportTransform[4] += evt.clientX - this.lastPosX;
                this.viewportTransform[5] += evt.clientY - this.lastPosY;
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
                this.requestRenderAll();
                return;
            } else if (!isDown || !mother.tagMode) return;

            var pointer = mother.canvas.getPointer(evt);
            rect.set({ 'width': Math.abs(pointer.x - startPosition.x), 'height': Math.abs(pointer.y - startPosition.y) });

            mother.bindingEvent(rect);
            mother.canvas.renderAll();

        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            if (isDragging) {
                isDragging = false;
                return;
            } else if (!mother.tagMode) return;

            if (!mother.addTag || !mother.editTag || this.isQc) return;
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
                mother.GetPercent(rect.get("left") + (rect.get('width')), mother.imageWidth),
                mother.GetPercent(rect.get("top") + (rect.get('height')), mother.imageHeight)
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
            // $("#deleteBtn").remove();
            if (!mother.addTag || !mother.editTag || mother.isQc) return;
        });

        this.canvas.on('object:modified', function (e) {
            if (!mother.addTag || !mother.editTag || mother.isQc) return;
            // mother.addDeleteBtn(e.target.oCoords.tr.x, e.target.oCoords.tr.y);
        });

        this.canvas.on('object:scaling', function (e) {
            if (!mother.addTag || !mother.editTag || mother.isQc) return;
        });

        this.canvas.on('object:rotating', function (e) {
            if (!mother.addTag || !mother.editTag || mother.isQc) return;
        });

        this.canvas.on('selection:cleared', function () {
            if (!mother.tagMode || !mother.addTag || !mother.editTag || mother.isQc) return;
            mother.tagMode = true;
        });

        this.canvas.on('object:moving', function (e) {
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
        tag.width = this.GetPercent((left + width), this.imageWidth);
        tag.height = this.GetPercent((top + height), this.imageHeight);

        console.log(tag);
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
        $(window).resize(function () {
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