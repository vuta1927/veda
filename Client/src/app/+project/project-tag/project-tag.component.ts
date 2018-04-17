import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Helpers } from '../../helpers';
import { ImageService } from '../services/image.service';
import { ConfigurationService } from '../../shared/services/configuration.service';
@Component({
    selector: 'app-project-tag',
    templateUrl: 'project-tag.component.html',
    styleUrls: ['project-tag.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class ProjecTagComponent {
    imageId: string = '';
    currentImage: any = {};
    apiUrl: string = '';
    canvas: any;
    tagMode: boolean = false;
    constructor(
        private route: ActivatedRoute,
        private imgService: ImageService,
        private configurationService: ConfigurationService
    ) {

    }

    ngOnInit() {
        this.apiUrl = this.configurationService.serverSettings.apiUrl;
        this.route.queryParams.filter(params => params.id).subscribe(params => {
            this.imageId = params.id;
            console.log(this.imageId);
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
        this.canvas = new fabric.Canvas('canvas', { selection: true });
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
        this.canvasPanning();//hold Alt
        this.setUpTagFunc();
    }

    setUpTagFunc() {
        let isDown: boolean = false;
        let startPosition: any = {};
        let rect: any = {};
        let mother = this;
        this.canvas.on('mouse:down', function (event) {
            if (!mother.tagMode) return;
            isDown = true;
            console.log(event.e.clientX, event.e.clientY);
            startPosition.x = event.e.clientX;
            startPosition.y = event.e.clientY;

            console.log(startPosition);

            rect = new fabric.Rect({
                left: event.e.clientX,
                top: event.e.clientY,
                width: 0,
                height: 0,
                stroke: 'red',
                strokeWidth: 3,
                fill: ''
            });
            mother.canvas.add(rect);
        });

        this.canvas.on('mouse:move', function (event) {
            if (!isDown || !mother.tagMode) return;

            rect.setWidth(Math.abs(event.e.clientX - startPosition.x));
            rect.setHeight(Math.abs(event.e.clientY - startPosition.y));

            mother.canvas.renderAll();
        });

        this.canvas.on('mouse:up', function () {
            isDown = false;
            mother.canvas.add(rect);
        });

        this.canvas.on('object:selected', function () {
            mother.tagMode = false;
        });

        this.canvas.on('object:selected', function () {
            mother.tagMode = false;
        });
        this.canvas.on('selection:cleared', function () {
            mother.tagMode = true;
        });
    }

    canvasPanning() {
        this.canvas.on('mouse:down', function (opt) {
            var evt = opt.e;
            if (evt.altKey === true) {
                this.isDragging = true;
                this.selection = false;
                this.lastPosX = evt.clientX;
                this.lastPosY = evt.clientY;
            }
        });
        this.canvas.on('mouse:move', function (opt) {
            if (this.isDragging) {
                var e = opt.e;
                this.viewportTransform[4] += e.clientX - this.lastPosX;
                this.viewportTransform[5] += e.clientY - this.lastPosY;
                this.requestRenderAll();
                this.lastPosX = e.clientX;
                this.lastPosY = e.clientY;
            }
        });
        this.canvas.on('mouse:up', function (opt) {
            this.isDragging = false;
            this.selection = true;
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
}