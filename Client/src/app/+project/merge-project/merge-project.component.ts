import { Component, OnInit, OnDestroy, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef } from '@angular/core';
import { ActivatedRoute, Router, RouterStateSnapshot } from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../../shared/services/security.service';
import { Constants } from '../../constants';
import { ClassService } from '../services/class.service';
import { ProjectUserService } from '../services/project-users.service';
import { MergeClass } from './merge-class/merge-class.component';
import { UpdateUserComponent } from './update-user/update-user.component';
import { DataService } from '../data.service';
import DataSource from 'devextreme/data/data_source';
import { Merge, MergeProjectUser, QcOption, FilterOptions, MergeKlass } from '../../shared/models/merge.model';
import { Class } from '../../shared/models/class.model';
import { ProjectUser } from '../../shared/models/project-user.model';
import { MergeService } from '../services/merge-project.service';
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from '@angular/forms';
import { FormService } from '../../shared/services/form.service';
import { Observable } from 'rxjs/Observable';
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from '../../shared/utils/form-errors/ngx-errors.service';
import { ProjectService } from '../project.service';
import { hub } from '../../shared/signalR/hub';

import { ConfigurationService } from '../../shared/services/configuration.service';
@Component({
    selector: 'app-merge-project',
    styleUrls: ['merge-project.component.css'],
    templateUrl: 'merge-project.component.html',
    encapsulation: ViewEncapsulation.None
})

export class MergeProject implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrids: DxDataGridComponent;
    // @ViewChild(DxDataGridComponent) dataGrid: DxDataGridComponent;
    selectedUsers: any[] = [];
    selectedClasses: any[] = [];
    classSource: any = {};
    userSource: any = {};
    newUserSource: any = {};
    orginClassDataSource: any;
    orginUserDataSource: any;
    projectName = '';
    levelTotal = 5;
    qcFilterOptions: QcOption[] = [];
    projects: string;
    mergeClasses: MergeKlass[] = [];
    form: FormGroup;
    btnSaveDisable = true;
    constructor(
        private route: Router,
        private activatedRoute: ActivatedRoute,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private classService: ClassService,
        private modalService: NgbModal,
        private dataService: DataService,
        private mergeService: MergeService,
        private projectUserSerivce: ProjectUserService,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private projectService: ProjectService,
        private config: ConfigurationService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        const mother = this;
        activatedRoute.params.subscribe(params => {
            mother.projects = params.projects;
            mother.classService.getClassProjects(params.projects)
                .toPromise().then(response => {
                    mother.dataService.changeClass(response.result);
                    mother.classSource = response.result;
                    mother.orginClassDataSource = Object.assign([], response.result);
                });
            mother.projectUserSerivce.GetProjectUsersForMerge(params.projects)
                .toPromise().then(response => {
                    mother.userSource = response.result;
                    mother.orginUserDataSource = Object.assign([], response.result);
                });
        });

        if (!hub._hubConnection) {
            hub.setupHub(this.config.serverSettings.apiUrl);
        }
    }

    ngOnInit(): void {
        this.createForm();
    }

    createForm() {
        this.form = this.formBuilder.group({
            projectName: ['', [Validators.required], this.validateProjectNameNotTaken.bind(this)]
        });
        this.ngxErrorsService.setDefaultMessage('nameTaken', { message: 'The project name already taken.' });
    }

    validateProjectNameNotTaken(control: AbstractControl) {

        if (!control.value) {
            return Observable.empty();
        }
        let result = null;
        result = this.projectService.getProjectByName(control.value).toPromise().then(Response => {
            if (Response && Response.result) {
                this.btnSaveDisable = true;
                return { nameTaken: true }
            } else {
                this.btnSaveDisable = false;
                return null;
            }
        });
        return result;
    }

    cancel(): void {
        this.route.navigate(['project']);
    }

    SaveAll(): void {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }

        const users: MergeProjectUser[] = [];
        const mergeData: Merge = new Merge();
        const filterOptions: FilterOptions = new FilterOptions();
        filterOptions.qcOptions = this.qcFilterOptions;

        this.userSource.forEach(user => {
            if (!user.roleName) {
                alert('Not all users have role !');
                return;
            }

            users.push({ userName: user.userName, roleName: user.roleName });
        });

        mergeData.projectName = this.projectName;
        mergeData.classes = this.classSource;
        mergeData.filterOptions = filterOptions;
        mergeData.users = users;
        mergeData.projects = this.projects.split(',');
        mergeData.mergeClasses = this.mergeClasses;
        mergeData.connectionId = hub.connectionId;

        const mother = this;
        Helpers.setLoading(true);
        this.mergeService.mergeProjcet(mergeData).toPromise().then(() => {
            Helpers.setLoading(false);
            mother.route.navigate(['project']);
        }).catch(err => { 
            alert(err.error.text);
            Helpers.setLoading(false);
        });
        mother.route.navigate(['project']);
    }

    qcLevelChange(e): void {
        const ele = e.target;
        const qcOpt = this.qcFilterOptions.find(x => x.index === ele.name);
        if (qcOpt) {
            qcOpt.value = ele.value;
        } else {
            this.qcFilterOptions.push({ index: ele.name, value: ele.value });
        }
    }

    userNameSelected(data: any): void {
        this.openProjectUserModal(data);
    }

    userSelectionChanged(data: any): void {

    }

    openProjectUserModal(data: any): void {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        const modalRef = this.modalService.open(UpdateUserComponent, config);

        modalRef.componentInstance.projectUser = data;
        console.log(data);
        const mother = this;
        modalRef.result.then(function () {
            mother.dataService.currentProjectUser.subscribe(p => {
                mother.userSource.forEach(e => {
                    if (e.userName == p.userName) {
                        e.roleName = p.roleName;
                    }
                });
            });
            // mother.dataGrid['first'].instance.refresh();
            // mother.classSource.reload();
        })
    }

    openMergeClassModal(): void {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        };

        const modalRef = this.modalService.open(MergeClass, config);
        if ( this.classSource )
            modalRef.componentInstance.classes = this.classSource;

        if (this.selectedClasses)
            modalRef.componentInstance.mergeClasses = this.selectedClasses;

        modalRef.componentInstance.projectName = this.projectName ? this.projectName : 'New Project';

        var mother = this;
        modalRef.result.then(function () {
            mother.dataService.currentClass.subscribe(p => {
                mother.classSource = p;
                // console.log(p);
            });

            mother.dataService.currentNewClass.subscribe(p => {
                if (mother.mergeClasses.find(x => x.newClass == p)) {
                    return;
                } else {
                    mother.mergeClasses.push({ oldClasses: mother.selectedClasses, newClass: p });
                }
            });
            // mother.dataGrid['first'].instance.refresh();
            // mother.classSource.reload();
        })
    }

    classSelectionChanged(data: any): void {
        // console.log(data, this.dataGrids);
        var selectedKeys = this.dataGrids['first'].instance.getSelectedRowKeys();
        var selectedClass = data.currentSelectedRowKeys[0];
        if (data.selectedRowsData.length > 1) {
            for (var i = 0; i < selectedKeys.length; i++) {
                if (
                    selectedKeys[i].project == selectedClass.project &&
                    selectedKeys[i].name == selectedClass.name &&
                    (selectedKeys[i].project == this.projectName || selectedKeys[i].project == 'New Project')) {
                    this.dataGrids['first'].instance.deselectRows([selectedKeys[i]]);
                }
                if (
                    (selectedKeys[i].project == this.projectName || selectedKeys[i].project == 'New Project') &&
                    (selectedKeys[i].project != selectedClass.project)
                ) {
                    this.dataGrids['first'].instance.deselectRows([selectedKeys[i]]);
                }
            }
        }
        this.selectedClasses = data.selectedRowsData;
    }

    projectNameChange(): void {
        var mother = this;
        if (this.classSource && this.classSource.length > 0) {
            this.classSource.forEach(e => {
                if (e.id == 0) {
                    e.project = mother.projectName;
                }
            });
        }
    }

    resetClass(): void {
        this.classSource = Object.assign([], this.orginClassDataSource);
    }

    resetUser(): void {
        this.orginUserDataSource.forEach(e => {
            e.roleName = '';
        });
        this.userSource = Object.assign([], this.orginUserDataSource);
    }

    removeClass(): void {
        if (!this.selectedClasses.length) {
            return;
        }

        this.selectedClasses.forEach(klass => {
            var e = this.classSource.find(x => x.name == klass.name);
            if (e) {
                this.classSource.splice(this.classSource.indexOf(e), 1);
            }
        });
    }
}