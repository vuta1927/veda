import { Component, OnInit, ViewChildren, ViewEncapsulation, ViewContainerRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag, ExcluseArea, Coodirnate } from '../../shared/models/tag.model';
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
    btnSaveEnabled: boolean = true;
    excluseMode: boolean = false;
    polygonPoints: any = [];
    startPoint: any;
    lines: any = [];
    ExcluseAreas: ExcluseArea[] = [];
    tagBtnClass: string = 'btn btn-outline-primary btn-sm m-btn m-btn--icon m-btn--wide';
    excluseBtnClass: string = 'btn btn-outline-danger btn-sm m-btn m-btn--icon m-btn--wide';
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
                        mother.classData.push(new ClassList(tag.id, tag.name, false, tag.classColor));
                    });
                    // mother.generateClassContainer();
                }
            }).catch(error => {
                mother.showError(error.error.text);
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

    classRadioChange(e) {
        if (this.selectedClass == null) return;

        var value = Number(e.target.value);

        if (e.target.checked) {
            this.classData.forEach(c => {
                if ($(`#${c.id}`).val() != value && $(`#${c.id}`).is(':checked') === true) {
                    $(`#${c.id}`).prop('checked', false);
                };
            });
        }

        this.tags.forEach(tag => {
            if (tag.index == this.selectedClass.index) {
                tag.classIds = [];

                if (e.target.checked) {
                    tag.classIds.push(value);
                }


                let color: string;
                if (tag.classIds.length > 0) {
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
        var mother = this;
        this.btnSaveEnabled = false;
        this.tagSerivce.saveTags(this.imageId, this.tags).toPromise().then(Response => {
            mother.showSuccess("All Tags saved!");
            mother.btnSaveEnabled = true;
        }).catch(errorResp => {
            mother.showError(errorResp.error.text);
            mother.btnSaveEnabled = true;
        });
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
        this.canvas = new fabric.Canvas('canvas', { selection: false });
        fabric.Circle.prototype.originX = fabric.Circle.prototype.originY = 'center';
        fabric.Line.prototype.originX = fabric.Line.prototype.originY = 'center';
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
        var btnLeft = x + 1;
        var btnTop = y;
        var deleteBtn = '<img src="https://cdn2.iconfinder.com/data/icons/icojoy/shadow/standart/png/24x24/001_05.png" id="deleteBtn" style="position:absolute;top:' + btnTop + 'px;left:' + btnLeft + 'px;cursor:pointer;width:25px;height:25px;"/>';
        $("#canvas-wrapper").append(deleteBtn);
    }

    drawTags() {
        let mother = this;
        this.tags.forEach(tag => {
            let x = this.GetValueFromPercent(tag.left, this.imageWidth);
            let y = this.GetValueFromPercent(tag.top, this.imageHeight);
            let width = this.GetValueFromPercent(tag.width, this.imageWidth);
            let height = this.GetValueFromPercent(tag.height, this.imageHeight);
            let color = tag.classIds.length > 0 ? mother.getColorByClass(tag.classIds[0]) : '#878787';
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

        this.canvas.on('mouse:down', function (event) {

            // $("#deleteBtn").remove();
            // if (!mother.canvas.getActiveObject()) {
            //     $("#deleteBtn").remove();
            // }

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
                        fill: "#ff26fb",
                        radius: 7,
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
                    stroke: '#ff26fb',
                    strokeDashArray: [5, 5],
                });
                mother.ExcluseAreas.push(new ExcluseArea())
                mother.polygonPoints.push(new fabric.Point(_x, _y));
                mother.lines.push(line);

                this.add(line);
            }


            //--- set up tag func ---
            if (!mother.tagMode || isDragging) return;
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
                stroke: '#878787',
                originColor: '#878787',
                strokeWidth: 2,
                fill: '',
                transparentCorners: false
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
            rect.set('width', Math.abs(pointer.x - startPosition.x));
            rect.set('height', Math.abs(pointer.y - startPosition.y));

            mother.bindingEvent(rect);
            mother.canvas.renderAll();

        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            if (isDragging) {
                isDragging = false;
                return;
            } else if (!mother.tagMode) return;

            mother.bindingEvent(rect);
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
                fill: '#878787',
                stroke: '#878787',
                originColor: '#878787',
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
                mother.addDeleteBtn(e.target.oCoords.tr.x, e.target.oCoords.tr.y);
            }
        });

        this.canvas.on('object:modified', function (e) {
            mother.addDeleteBtn(e.target.oCoords.tr.x, e.target.oCoords.tr.y);
        });

        this.canvas.on('object:scaling', function (e) {
            $("#deleteBtn").remove();
        });

        this.canvas.on('object:rotating', function (e) {
            $("#deleteBtn").remove();
        });

        $(document).on('click', "#deleteBtn", function () {

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
        this.polygonPoints.push(new fabric.Point(this.polygonPoints[0].x, this.polygonPoints[0].y));

        return new fabric.Polygon(this.polygonPoints.slice(), {
            left: left,
            top: top,
            fill: '#000000',
            stroke: 'black',
            name: 'ex-' + mother.generateId(),
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

        this.tags.forEach(tag => {
            if (tag.index == target.index) {
                tag.top = this.GetPercent(top, this.imageHeight);
                tag.left = this.GetPercent(left, this.imageWidth);
                tag.width = this.GetPercent((left + width), this.imageWidth);
                tag.height = this.GetPercent((top + height), this.imageHeight);
            };
        });
    }

    onTagSelectedEvent(target) {
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
        let index: number = 1;
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