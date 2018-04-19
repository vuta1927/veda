import { Component, ViewEncapsulation, ViewChildren, OnInit, Input, ViewContainerRef } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { ProjectForView } from '../../../shared/models/project.model';
import { FormService } from "../../../shared/services/form.service";
import { DataService } from '../data.service';
import { ImageService } from '../../services/image.service';
import { ProjectService } from '../../project.service';
import { Observable } from "rxjs/Observable";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { IAppCoreResponse } from "../../../shared/models/appcore-response.model";
import { Helpers } from '../../../helpers';
import * as _ from 'lodash';

import { HttpClient, HttpRequest, HttpEventType, HttpResponse } from '@angular/common/http';
import { ConfigurationService } from "../../../shared/services/configuration.service";


import { DxDataGridComponent } from 'devextreme-angular';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { Iimage } from '../../../shared/models/image.model';

@Component({
    selector: 'app-project-images',
    templateUrl: './project-images.component.html',
    styleUrls: ['./project-images.component.css'],
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class ProjectImagesComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent;
    dataSource: any = {};
    selectedImages: any[] = [];
    images: any[] = [];
    currentProject: ProjectForView = new ProjectForView("0");
    btnSaveDisable: boolean = true;
    form: FormGroup;
    messageHeader: string;
    message: string;
    progress: number;
    uploadfiles: any = [];
    apiUrl: string = '';
    constructor(
        private formBuilder: FormBuilder,
        private toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private dataService: DataService,
        private imgService: ImageService,
        private projectService: ProjectService,
        private ngxErrorsService: NgxErrorsService,
        public formService: FormService,
        private http: HttpClient,
        private configurationService: ConfigurationService,
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        this.apiUrl = configurationService.serverSettings.apiUrl + '/';
        var mother = this;
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                var params = '';

                params += loadOptions.skip || 0;
                params += '/' + loadOptions.take || 12;

                return imgService.getImages(mother.currentProject.id, params)
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
        this.uploadfiles = [];
        var mother = this;
        this.dataService.currentProject.subscribe(p => {
            this.currentProject = p;
            this.imgService.getImages(p.id, '0/12').toPromise().then(Response => {
                if (Response && Response.result) {
                    this.dataSource = Response.result;
                    mother.dataGrid["first"].instance.refresh();
                }
            });
        }, error => {
            console.log(error)
        });

    }

    appendUploadFiles(files) {
        this.uploadfiles = files;
    }
    selectionChanged(data: any){
        this.selectedImages = data.selectedRowsData;
    }

    deleteSelectedImages() {
        Helpers.setLoading(true);
        let ids = '';
        console.log(this.selectedImages);
        for (let i = 0; i < this.selectedImages.length; i++) {
            if (i != (this.selectedImages.length - 1))
                ids += this.selectedImages[i].id + '_';
            else
                ids += this.selectedImages[i].id;
        }

        // console.log(ids);
        var mother = this;
        this.imgService.DeleteImage(ids).toPromise().then(Response => {
            Helpers.setLoading(false);
            mother.dataGrid["first"].instance.refresh();
            if (Response && Response.result) {
                let error = Response.result.split('#')[1];
                mother.showError(error);
            }else{
                mother.showInfo("Images deleted");
            }
        });
    }


    upload() {
        if (this.uploadfiles.length === 0)
            return;

        const formData = new FormData();

        for (let file of this.uploadfiles)
            formData.append(file.name, file);

        this.projectService.UploadImg(this.currentProject.id, formData).toPromise().then(Response => {
            if (Response && Response.result) {
                $('#errorMessage').css("display", "none");
                this.message = '';
                this.uploadfiles = [];
                this.showSuccess("Files uploaded !");
                $('#successMessage').css("display", "block");
                this.dataGrid["first"].instance.refresh();
            }
        }).catch(res => {
            $('#successMessage').css("display", "none");
            this.messageHeader = 'Upload Error';
            this.message = res['error'].text;
            $('#errorMessage').css("display", "block");
            this.showError(res['error'].text);
        })
    }

    clear() {
        this.uploadfiles = [];
        $('#successMessage').css("display", "none");
        $('#errorMessage').css("display", "none");
    }

    uploadFile(file) {
        if (file.size == 0)
            return;

        const formData = new FormData();

        formData.append(file.name, file);

        this.projectService.UploadImg(this.currentProject.id, formData).toPromise().then(Response => {
            if (Response && Response.result) {
                this.removeFile(file);
                $('#errorMessage').css("display", "none");
                this.message = '';
                this.showSuccess("Files uploaded !");
                this.dataGrid["first"].instance.refresh();
            }
        }).catch(res => {
            this.messageHeader = 'Upload Error';
            this.message = res['error'].text;
            $('#errorMessage').css("display", "block");
            this.showError(res['error'].text);
        })
    }

    removeFile(file) {
        let tempFiles = [];
        for (let f of this.uploadfiles) {
            if (f.name == file.name) continue;
            tempFiles.push(f);
        }
        this.uploadfiles = tempFiles;
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