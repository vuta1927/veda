import { Component, OnInit, ViewChildren, ViewEncapsulation, ViewContainerRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag } from '../../shared/models/tag.model';
import { TagService } from '../services/tag.service';
import { ClassList } from '../../shared/models/class.model';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
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
    selectedClass: Tag;
    tags: Array<Tag> = new Array<Tag>();
    canvasTags: any = [];
    canvasLabels: any = [];
    imageWidth: number = 0;
    imageHeight: number = 0;
    imageUrl: string = '';

    constructor(
        private route: ActivatedRoute,
        private imgService: ImageService,
        private classService: ClassService,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private configurationService: ConfigurationService,
        private tagSerivce: TagService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        let mother = this;
    }

    ngOnInit() {
        this.apiUrl = this.configurationService.serverSettings.apiUrl;
        let mother = this;

        this.route.queryParams.subscribe(params => {
            this.imageId = params.id;
            this.projectId = params.project;

            this.classService.getClasses(this.projectId).toPromise().then(Response => {
                if (Response && Response.result) {
                    Response.result.forEach(tag => {
                        mother.classData.push(new ClassList(tag.id, tag.name));
                    });
                    // mother.generateClassContainer();
                }
            }).catch(error => {
                this.showError(error.error.text);
            });

            this.tagSerivce.getTags(this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    this.tags = Response.result;

                    this.imgService.getImageById(this.imageId).toPromise().then(Response => {
                        if (Response && Response.result) {
                            this.currentImage = Response.result;
                            this.imageUrl = this.apiUrl + '/' + this.currentImage.path;
                            this.initCanvas();
                        }
                    });
                }
            });
        });
    }

    switchTagMode() {
        if (this.tagMode)
            this.tagMode = false;
        else
            this.tagMode = true;
    }

    classCheckBoxChange(e) {
        if (this.selectedClass == null) return;

        var value = Number(e.target.value);
        this.tags.forEach(tag => {
            if (tag.index == this.selectedClass.index) {
                if (tag.classIds.indexOf(value) != -1) {
                    if (!e.target.checked) {
                        tag.classIds.splice(tag.classIds.indexOf(value), 1);
                    }
                } else {
                    if (e.target.checked) {
                        tag.classIds.push(value);
                    }
                }

                let color: string;
                if (tag.classIds.length > 0) {
                    color = 'red';
                } else {
                    color = 'yellow';
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
            }
        });

        this.classData.forEach(c => {
            if (c.id == value) {
                if (e.target.checked)
                    c.checked = true;
                else
                    c.checked = false;

            }
        });
    }

    saveChange() {
        this.tagSerivce.saveTags(this.imageId, this.tags).toPromise().then(Response => {
            if (!Response || !Response.result)
                this.showSuccess("All Tags saved!");
        }).catch(errorResp => {
            this.showError(errorResp.error.text);
        });
    }


    initCanvas() {
        var mother = this;
        this.canvas = new fabric.Canvas('canvas', { selection: false });

        var HideControls = {
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
        fabric.Image.fromURL(this.apiUrl + '/' + this.currentImage.path, function (img) {
            img.selectable = false;
            img.setControlsVisibility(HideControls);
            img.set({
                name: 'background',
                left: 0,
                top: 0
            });
            mother.canvas.add(img);
            mother.canvas.sendToBack(img);
            mother.canvas.renderAll();
            mother.imageHeight = img.height;
            mother.imageWidth = img.width;
            mother.drawTags();
        });

        this.autoResizeCanvas();
        this.setupZoomFunc();
        this.setUpMouseEvent();
    }

    addDeleteBtn(x, y) {
        $("#deleteBtn").remove();
        var btnLeft = x+0.5;
        var btnTop = y;
        var deleteBtn = '<img src="https://cdn2.iconfinder.com/data/icons/icojoy/shadow/standart/png/24x24/001_05.png" id="deleteBtn" style="position:absolute;top:' + btnTop + 'px;left:' + btnLeft + 'px;cursor:pointer;width:25px;height:25px;"/>';
        $("#canvas-wrapper").append(deleteBtn);
    }

    drawTags() {
        console.log(this.tags);
        let mother = this;
        this.tags.forEach(tag => {
            let x = this.GetValueFromPercent(tag.left, this.imageWidth);
            let y = this.GetValueFromPercent(tag.top, this.imageHeight);
            let width = this.GetValueFromPercent(tag.width, this.imageWidth);
            let height = this.GetValueFromPercent(tag.height, this.imageHeight);
            console.log('x', x, 'y', y, 'width', width, 'height', height);
            let color = tag.classIds.length > 0 ? 'red' : 'yellow';
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
                transparentCorners: false
            });
            rect.on('selected', function (e) {
                mother.onTagSelected(this);
            })
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

        this.canvas.on('mouse:down', function (event) {
            
            // $("#deleteBtn").remove();
            // --- set up panning func ---
            // if (!mother.canvas.getActiveObject()) {
            //     $("#deleteBtn").remove();
            // }
            var evt = event.e;
            if (evt.altKey === true) {
                isDragging = true;
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
                return;
            }
            //--- end pan func ---

            //--- set up tag func ---
            if (!mother.tagMode || isDragging) return;
            isDown = true;
            var pointer = mother.canvas.getPointer(evt);
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
                stroke: 'yellow',
                originColor: 'yellow',
                strokeWidth: 2,
                fill: '',
                transparentCorners: false
            });
            mother.canvas.add(rect);
        });

        this.canvas.on('mouse:move', function (event) {
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
            rect.set('width', Math.abs(pointer.x - startPosition.x));
            rect.set('height', Math.abs(pointer.y - startPosition.y));

            rect.on('selected', function (event) {
                mother.onTagSelected(this);
            });
            mother.canvas.renderAll();

        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            if (isDragging) {
                isDragging = false;
                return;
            } else if (!mother.tagMode) return;

            rect.on('selected', function (e) {
                mother.onTagSelected(this);
            })

            mother.canvas.add(rect);
            mother.canvasTags.push(rect);

            let index = rect.get('index');
            let id = rect.get('id');
            mother.tags.push(new Tag(
                -1,
                index,
                mother.imageId,
                [],
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
                fill: 'yellow',
                stroke: 'yellow',
                originColor: 'yellow',
                strokeWidth: 0,
                hasRotatingPoint: false,
                centerTransform: true,
                selectable: false
            });
            // console.log(rect.get("top"), rect.get("left"), rect.get("width"), rect.get("height"));
            // console.log(mother.tags)
            mother.canvas.add(label);
            mother.canvasLabels.push(label);
        });

        this.canvas.on('object:selected', function (e) {
            mother.tagMode = false;
            // $("#deleteBtn").remove();
            if (e.target.get('name').split('-')[0] == 'rect') {
                mother.addDeleteBtn(e.target.oCoords.tr.x * e.target.scaleX, e.target.oCoords.tr.y* e.target.scaleY);
            }
        });

        this.canvas.on('object:modified', function (e) {
            mother.addDeleteBtn(e.target.oCoords.tr.x * e.target.scaleX, e.target.oCoords.tr.y* e.target.scaleY);
        });

        this.canvas.on('object:scaling', function (e) {
            $("#deleteBtn").remove();
        });

        this.canvas.on('object:rotating', function (e) {
            $("#deleteBtn").remove();
        });

        $(document).on('click', "#deleteBtn", function () {
            console.log('delete clicked');
        });

        this.canvas.on('selection:cleared', function () {
            if (!mother.tagMode) return;
            mother.tagMode = true;
        });

        this.canvas.on('object:moving', function (e) {
            $("#deleteBtn").remove();
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
            $("#deleteBtn").remove();
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

    onTagSelected(target) {
        this.tags.forEach(tag => {
            if (tag.index == target.index) {
                this.selectedClass = tag;

                for (let i = 0; i < this.classData.length; i++) {
                    if (tag.classIds.indexOf(this.classData[i].id) != -1) {
                        this.classData[i].checked = true;
                    } else {
                        this.classData[i].checked = false;
                    }
                }
            }
        });
    }

    canvasPanning() {
        var mother = this;
        this.canvas.on('mouse:down', function (opt) {
            var evt = opt.e;
            if (evt.altKey === true) {
                mother.isDragging = true;
                mother.selection = false;
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
            }
        });
        this.canvas.on('mouse:move', function (opt) {
            if (mother.isDragging) {
                var e = opt.e;
                this.viewportTransform[4] += e.clientX - this.lastPosX;
                this.viewportTransform[5] += e.clientY - this.lastPosY;
                this.requestRenderAll();
                this.lastPosX = e.clientX;
                this.lastPosY = e.clientY;
            }
        });
        this.canvas.on('mouse:up', function (opt) {

            mother.isDragging = false;
            mother.selection = true;
        });
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
            $("#deleteBtn").remove();
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
        let index: number = 0;
        this.tags.forEach(tag => {
            index += 1;
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