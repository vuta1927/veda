import { Component, OnInit, ViewChildren , ViewEncapsulation, ViewContainerRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ClassService } from '../services/class.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { Tag } from '../../shared/models/tag.model';
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
    dataSource: any = {};
    currentImage: any = {};
    apiUrl: string = '';
    canvas: any;
    tagMode: boolean = false;
    isDragging: boolean = false;
    selection: boolean = false;
    projectId: string = null;
    tags: Array<Tag> = new Array<Tag>();
    rects: any = [];
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
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                var params = '';

                params += loadOptions.skip || 0;
                params += '/' + loadOptions.take || 12;

                return mother.classService.getClasses(this.projectId, params)
                    .toPromise()
                    .then(response => {
                        return {
                            data: response.result,
                            totalCount: response.result.length
                        }
                    })
                    .catch(error => { throw 'Data Loading Error' });
            }
        });
    }

    ngOnInit() {
        this.apiUrl = this.configurationService.serverSettings.apiUrl;
        let mother = this;
        this.route.queryParams.subscribe(params => {
            this.imageId = params.id;
            this.projectId = params.project;
            
            this.classService.getClasses(this.projectId, '0/12').toPromise().then(Response=>{
                if(Response && Response.result){
                    mother.dataSource = Response.result;
                }
            }).catch(error=>{
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
                name: 'rect-'+id,
                left: pointer.x,
                top: pointer.y,
                width: 0,
                height: 0,
                stroke: 'red',
                strokeWidth: 1,
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

            mother.canvas.renderAll();

        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            if (isDragging) {
                isDragging = false;
                return;
            } else if (!mother.tagMode) return;

            mother.canvas.add(rect);
            mother.rects.push(rect);

            let id = rect.get('id');
            mother.tags.push(new Tag(id, mother.imageId, [], 0, rect.get("height"), rect.get("width")));
            let label = new fabric.IText(id + '.', {
                name: 'lbl-' + id,
                parentName: rect.get('name'),
                left: rect.get('left'),
                top: rect.get('top') - 30,
                fontFamily: "calibri",
                fontSize: 25,
                fill: 'red',
                stroke: 'red',
                strokeWidth: 0,
                hasRotatingPoint: false,
                centerTransform: true,
                selectable: false
            });
            mother.canvas.add(label);
            console.log(mother.tags);
        });

        this.canvas.on('object:selected', function () {
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
            if(target.name.split('-')[0] == 'rect'){
                allObject.forEach(obj => {

                    var names = obj.get('name');
                    var parent = obj.get('parentName');

                    if(names.split('-')[0] == 'lbl' && parent == target.name){
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
            if(target.name.split('-')[0] == 'rect'){
                allObject.forEach(obj => {
                    var names = obj.get('name');
                    var parent = obj.get('parentName');

                    if(names.split('-')[0] == 'lbl' && parent == target.name){
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