import { Component, ViewEncapsulation, ViewChildren, OnInit, Input, ViewContainerRef } from '@angular/core';
import { Router} from '@angular/router';
import { FormControl, FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { ProjectForView } from '../../../shared/models/project.model';
import { FormService } from "../../../shared/services/form.service";
import { DataService } from '../data.service';
import { ProjectUserService } from '../../services/project-users.service';
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
import DataSource from 'devextreme/data/data_source';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CreateUpdateProjectUserComponent } from './create-update-project-user/create-update-project-user.component';
import { IProjectUser, ProjectUserForAdd } from '../../../shared/models/project-user.model';
import { IUser } from '../../../shared/models/user.model';
import { RoleForView } from '../../../shared/models/role.model';
import { UsersService } from '../../../+administrator/users/users.service';
import { RolesService } from '../../../+administrator/roles/roles.service';
import swal from 'sweetalert2';
@Component({
    selector: 'app-project-users',
    templateUrl: './project-users.component.html',
    styleUrls: ['./project-users.component.css'],
    encapsulation: ViewEncapsulation.None,
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class ProjectUsersComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent;
    dataSource: any = {};
    selectedUsers: any[] = [];
    currentProject: ProjectForView = new ProjectForView("0");
    form: FormGroup;
    messageHeader: string;
    message: string;
    apiUrl: string = '';
    constructor(
        private modalService: NgbModal,
        private formBuilder: FormBuilder,
        private toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private dataService: DataService,
        private projectUserService: ProjectUserService,
        private projectService: ProjectService,
        private ngxErrorsService: NgxErrorsService,
        public formService: FormService,
        private http: HttpClient,
        private configurationService: ConfigurationService,
        private roleService: RolesService,
        private userService: UsersService,
        private router: Router
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        this.apiUrl = configurationService.serverSettings.apiUrl + '/';
    }

    ngOnInit() {
        var mother = this;
        this.dataService.currentProject.subscribe(p => {
            this.currentProject = p;
            this.dataSource = new DataSource({
                store: new CustomStore({
                    key: 'id',
                    load: function (loadOptions: any) {
                        let params = '';
                        params += loadOptions.skip || 0;
                        params += '/';
                        params += loadOptions.take || 12;

                        return mother.projectUserService.getProjectUsers(p.id, params)
                            .toPromise()
                            .then(response => {
                                return response.result;
                            })
                            .catch(error => { 
                                if(error.status == 401 || error.status == 403){
                                    mother.router.navigate(['#']);
                                    return;
                                };
                                throw 'Data Loading Error';
                             });
                    },
                    totalCount:function(){
                        return mother.projectUserService.getTotal(p.id).toPromise().then(resp => {
                            return resp.result;
                        })
                    }
                })
            })
            // this.projectUserService.getProjectUsers(p.id, '0/12').toPromise().then(Response => {
            //     if (Response && Response.result) {
            //         this.dataSource = Response.result;
            //         mother.dataGrid["first"].instance.refresh();
            //     }
            // });
        }, error => {
            console.log(error)
        });

    }

    selectionChanged(data: any) {
        this.selectedUsers = data.selectedRowsData;
    }

    deleteSelectedImages() {
        Helpers.setLoading(true);
        let ids = '';
        console.log(this.selectedUsers);
        for (let i = 0; i < this.selectedUsers.length; i++) {
            if (i != (this.selectedUsers.length - 1))
                ids += this.selectedUsers[i].id + '_';
            else
                ids += this.selectedUsers[i].id;
        }

        // console.log(ids);
        var mother = this;
        this.projectUserService.DeleteProjectUser(ids).toPromise().then(Response => {
            Helpers.setLoading(false);
            mother.dataGrid["first"].instance.refresh();
            if (Response && Response.result) {
                mother.showInfo("User deleted");
            }
        }).catch(res => {
            if(res.status == 401 || res.status == 403){
                mother.router.navigate(['#']);
                return;
            };
            swal({text:"Cant delete there users", type:"error"});
        });
    }

    addUser() {
        this.openCreateOrUpdateModal();
    }

    openCreateOrUpdateModal(projectUser?) {
        Helpers.setLoading(true);
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }

        var mother = this;
        var users = [];
        var roles = [];
        this.userService.getUsers().toPromise().then(Response => {
            if (Response && Response.result) {
                users = Response.result;
                this.roleService.getProjectRoles().toPromise().then(res => {
                    if (res && res.result) {
                        roles = res.result;
                        const modalRef = this.modalService.open(CreateUpdateProjectUserComponent, config);
                        modalRef.componentInstance.users = Response.result;
                        modalRef.componentInstance.roles = res.result;
                        Helpers.setLoading(false);
                        modalRef.result.then(function () {
                            mother.dataGrid["first"].instance.refresh();
                        })
                    } else {
                        return;
                    }

                })
            } else {
                return;
            }
        });
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