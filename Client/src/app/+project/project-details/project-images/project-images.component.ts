import { Component, ViewEncapsulation, ViewChildren, OnInit, Input, ViewContainerRef } from '@angular/core';
import { Router } from '@angular/router';

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
import { SecurityService } from '../../../shared/services/security.service';
import { Constants } from '../../../constants';

import { DxDataGridComponent } from 'devextreme-angular';
import DataSource from 'devextreme/data/data_source';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { Iimage } from '../../../shared/models/image.model';
import { resolve, reject } from 'q';

import { SignalRService } from '../../services/signalr.service';
import { MessageTypes } from '../messageType';
import { HubConnection } from '@aspnet/signalr';
import swal from 'sweetalert2';
@Component({
    selector: 'app-project-images',
    templateUrl: './project-images.component.html',
    styleUrls: ['./project-images.component.css'],
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class ProjectImagesComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent;
    dataSource: any = {};
    rawData: any = {};
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
    uploadProgress: number = 0;
    uploading: boolean = false;
    uploadedFile: number = 0;
    totalFile: number = 0;
    messageTypes: MessageTypes = new MessageTypes();
    _hubConnection: HubConnection;
    viewProject: boolean = false;
    editProject: boolean = false;
    addProject: boolean = false;
    viewImage: boolean = false;
    addImage: boolean = false;
    deleteImage: boolean = false;
    isQc: boolean = false;
    userUsing: any;

    constructor(
        private formBuilder: FormBuilder,
        private vcr: ViewContainerRef,
        private dataService: DataService,
        private imgService: ImageService,
        private projectService: ProjectService,
        private ngxErrorsService: NgxErrorsService,
        public formService: FormService,
        private http: HttpClient,
        private configurationService: ConfigurationService,
        private securityService: SecurityService,
        private signalService: SignalRService,
        private router: Router
    ) {
        this.apiUrl = configurationService.serverSettings.apiUrl + '/';
    }

    ngOnInit() {
        this.viewProject = this.securityService.IsGranted(Constants.viewProject);
        this.editProject = this.securityService.IsGranted(Constants.editProject);
        this.addProject = this.securityService.IsGranted(Constants.addProject);
        this.viewImage = this.securityService.IsGranted(Constants.viewImage);
        this.addImage = this.securityService.IsGranted(Constants.addImage);
        this.deleteImage = this.securityService.IsGranted(Constants.deleteImage);

        this.isQc = this.securityService.isInRole(Constants.QuantityCheck);

        this.uploadfiles = [];
        var mother = this;
        this.setupHub();
        Helpers.setLoading(true);
        this.dataService.currentProject.subscribe(p => {
            this.currentProject = p;
            this.dataSource = new DataSource({
                store: new CustomStore({
                    key: "id",
                    load: function (loadOptions: any) {
                        let params = '';

                        params += loadOptions.skip || 0;
                        params += '/';
                        params += loadOptions.take || 12;

                        return mother.imgService.getImages(p.id, params)
                            .toPromise().then(response => {
                                return mother.imgService.getTotal(p.id).toPromise().then(resp => {
                                    mother.rawData = response.result;
                                    Helpers.setLoading(false);
                                    if (resp.result) {
                                        return {
                                            data: mother.rawData,
                                            totalCount: resp.result,
                                        }
                                    } else {
                                        return {
                                            data: mother.rawData,
                                            totalCount: 0
                                        }
                                    }
                                })
                            }).catch(error => { throw 'Data Loading Error' });
                    }
                })
            })
        }, error => {
            swal({title:'', text: error.error? error.error.text: error.message}).then(()=>{
                Helpers.setLoading(false);
            })
        });

    }

    setupHub() {
        var mother = this;
        this._hubConnection = new HubConnection(this.apiUrl + 'hubs/image');
        this._hubConnection
            .start()
            .then(() => console.log('connection started!'))
            .catch(err => console.log('Error while establishing connection (' + this.apiUrl + 'hubs/image' + ') !'));
        this._hubConnection.on("userUsingInfo", data => {
            console.log("userUsingInfo", data);
            mother.updateUserUsing(data);
        });
    }

    updateUserUsing(userUsingInfoData) {
        var mother = this;
        userUsingInfoData.forEach(data => {
            // var obj = mother.rawData.find(x=>x.id == data.imageId);
            if(mother.rawData){
                mother.rawData.forEach(dt => {
                    if (dt.id == data.imageId) {
                        dt.userUsing = data.userName;
                    }
                });
            }
            
            // if(obj){
            //     obj.userUsing = data.userName;
            // }
        });
        this.dataSource.reload();
    }
    
    customPipeQcStatus(){

    }

    appendUploadFiles(files) {
        this.uploadfiles = files;
    }
    selectionChanged(data: any) {
        this.selectedImages = data.selectedRowsData;
    }

    deleteSelectedImages() {
        Helpers.setLoading(true);
        let ids = '';
        for (let i = 0; i < this.selectedImages.length; i++) {
            if (i != (this.selectedImages.length - 1))
                ids += this.selectedImages[i].id + '_';
            else
                ids += this.selectedImages[i].id;
        }

        var mother = this;
        this.imgService.DeleteImage(ids).toPromise().then(Response => {
            Helpers.setLoading(false);
            // mother.dataGrid["first"].instance.refresh();
            mother.dataSource.reload();
            if (Response && Response.result) {
                let error = Response.result.split('#')[1];
                swal({
                    title: '', text: error, type: 'error'
                });
            } else {
                swal({
                    title: '', text: 'Images deleted', type: 'success'
                });
            }
        });
    }

    startTrainning() {
        this.router.navigateByUrl('/project-tag?project=' + this.currentProject.id);
    }

    upload() {
        // if (this.uploadfiles.length === 0)
        //     return;

        this.uploading = true;
        if (!this.totalFile)
            this.totalFile = this.uploadfiles.length;
        // let totalFile = this.uploadfiles.length;

        if (this.uploadfiles.length > 0 && this.uploadedFile <= (this.totalFile - 1)) {
            let file = this.uploadfiles[this.uploadedFile];
            const formData = new FormData();
            formData.append(file.name, file);
            this.projectService.UploadImg(this.currentProject.id, formData).toPromise().then(Response => {
                if (Response && Response.result) {
                    this.message = '';
                    $('#errorMessage').css("display", "none");
                    // this.showSuccess("Files uploaded !");
                    this.uploadedFile += 1;
                    // this.uploadProgress = Math.round((uploadedFile / totalFile) * 100);
                    // console.log(this.uploadProgress, (uploadedFile));
                    // if (this.uploadProgress == 100) {

                    //     this.uploading = false;
                    //     this.uploadProgress = 0;
                    //     $('#successMessage').css("display", "block");
                    // }
                    this.upload();
                }
            }).catch(res => {
                $('#successMessage').css("display", "none");
                this.messageHeader = 'Upload Error';
                this.message = res['error'].text;
                $('#errorMessage').css("display", "block");
                swal({
                    title: '', text: res['error']? res['error'].text: res.message, type: 'error'
                });
            })
        } else {
            this.uploadfiles = [];
            this.totalFile = 0;
            this.uploadedFile = 0;
            this.dataSource.reload();
        }

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
                swal({
                    title: '', text: "Files uploaded !", type: 'success'
                });
                this.dataGrid["first"].instance.refresh();
            }
        }).catch(res => {
            this.messageHeader = 'Upload Error';
            this.message = res['error'].text;
            $('#errorMessage').css("display", "block");
            swal({
                title: '', text: res['error']? res['error'].text: res.message, type: 'error'
            });
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

}