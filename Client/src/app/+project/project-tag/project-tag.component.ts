import { Component, OnInit, ViewChildren, ViewEncapsulation, ViewContainerRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag } from '../../shared/models/tag.model';
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
    constructor(
        private route: ActivatedRoute,
        private imgService: ImageService,
        private classService: ClassService,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private configurationService: ConfigurationService
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

            this.imgService.getImageById(this.imageId).toPromise().then(Response => {
                if (Response && Response.result) {
                    this.currentImage = Response.result;
                    this.initCanvas();
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
            if (tag.id == this.selectedClass.id) {
                if (tag.classIds.indexOf(value) != -1) {
                    if (!e.target.checked) {
                        tag.classIds.splice(tag.classIds.indexOf(value), 1);
                    }
                } else {
                    if (e.target.checked) {
                        tag.classIds.push(value);
                    }
                }

                let color:string;
                if(tag.classIds.length > 0){
                    color = 'red';
                }else{
                    color = 'yellow';
                }

                this.canvasTags.forEach(cvTag => {
                    cvTag.set('stroke',color);
                    cvTag.set('originColor', color);
                });

                this.canvasLabels.forEach(cvLabel => {
                    cvLabel.set('originColor', color);
                    cvLabel.set('stroke',color);
                    cvLabel.set('fill',color);
                });
            }
        });

        this.classData.forEach(c => {
            if (c.id == value) {
                if(e.target.checked)
                    c.checked = true;
                else
                    c.checked = false;

            }
        });
    }

    initCanvas() {
        var mother = this;
        this.canvas = new fabric.Canvas('canvas', { selection: false });
        fabric.Image.fromURL(this.apiUrl + '/' + this.currentImage.path, function (img) {
            img.selectable = false;
            img.set({
                name: 'background',
                left: 0,
                top: 0
            });
            mother.canvas.add(img);
            mother.canvas.sendToBack(img);
        });
        let image = new Image();
        image.src = this.apiUrl + '/' + this.currentImage.path;
        image.onload = function(){
            mother.imageWidth = this['height'];
            mother.imageWidth = this['width'];
        }
        console.log(image.width, image.height);
        this.autoResizeCanvas();
        this.setupZoomFunc();
        this.setUpMouseEvent();
    }

    setUpMouseEvent() {
        let isDown: boolean = false;
        let startPosition: any = {};
        let rect: any = {};
        let mother = this;
        let isDragging: boolean = false;
        this.canvas.on('mouse:down', function (event) {
            // --- set up panning func ---
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

            let id = mother.generateId();

            rect = new fabric.Rect({
                id: id,
                name: 'rect-' + id,
                left: pointer.x,
                top: pointer.y,
                width: 0,
                height: 0,
                stroke: 'yellow',
                originColor: 'yellow',
                strokeWidth: 2,
                fill: ''
            });
            mother.canvas.add(rect);
        });

        this.canvas.on('mouse:move', function (event) {
            if (isDragging) {
                var evt = event.e;
                this.viewportTransform[4] += evt.clientX - this.lastPosX;
                this.viewportTransform[5] += evt.clientY - this.lastPosY;
                this.requestRenderAll();
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
                return;
            } else if (!isDown || !mother.tagMode) return;

            var pointer = mother.canvas.getPointer(evt);
            rect.set('width', Math.abs(pointer.x - startPosition.x));
            rect.set('height', Math.abs(pointer.y - startPosition.y));

            rect.on('selected', function (event) {
                mother.tags.forEach(tag => {
                    if (tag.id == this.id) {
                        mother.selectedClass = tag;

                        for (let i = 0; i < mother.classData.length; i++) {
                            if (tag.classIds.indexOf(mother.classData[i].id) != -1) {
                                mother.classData[i].checked = true;
                            } else {
                                mother.classData[i].checked = false;
                            }
                        }
                    }
                });

                let allObject = mother.canvas.getObjects();

                allObject.forEach(obj => {
                    obj.set("stroke", obj.originColor);
                    if(obj.name.split('-')[0] == 'lbl'){
                        obj.set('fill', obj.originColor);
                    }
                    if(obj.id == this.id){
                        obj.set("stroke", '#ff44f5');
                        if(obj.name.split('-')[0] == 'lbl'){
                            obj.set('fill', '#ff44f5');
                        }
                    }
                });

                
            });
            mother.canvas.renderAll();

        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            if (isDragging) {
                isDragging = false;
                return;
            } else if (!mother.tagMode) return;

            mother.canvas.add(rect);
            mother.canvasTags.push(rect);

            let id = rect.get('id');
            mother.tags.push(new Tag(id, mother.imageId, [], 0, rect.get("height"), rect.get("width")));
            let label = new fabric.IText(id + '.', {
                name: 'lbl-' + id,
                id: id,
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
            mother.canvas.add(label);
            mother.canvasLabels.push(label);
        });

        this.canvas.on('object:selected', function (e) {
            // mother.tagMode = false;
        });

        this.canvas.on('selection:cleared', function () {
            if (!mother.tagMode) return;
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
        let id: number = 0;
        this.tags.forEach(tag => {
            id += 1;
        });
        return id;
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
}