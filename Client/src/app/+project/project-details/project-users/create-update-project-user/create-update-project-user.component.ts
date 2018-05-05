import { Component, Input, OnInit, ViewContainerRef } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../../shared/services/form.service";
import { matchOtherValidator } from "../../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../../shared/utils/form-errors/ngx-errors.service";
import { DxTextBoxModule, DxFormModule } from 'devextreme-angular';
import { ProjectUserForAdd } from '../../../../shared/models/project-user.model';
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { SecurityService } from '../../../../shared/services/security.service';
import { ProjectUserService } from '../../../services/project-users.service';
import { Helpers } from '../../../../helpers';
import { DataService } from '../../data.service';
@Component({
    selector: 'create-update-project-user',
    templateUrl: './create-update-project-user.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class CreateUpdateProjectUserComponent implements OnInit {
    form: FormGroup;
    projectUser: any = null;
    currentProject: any = {};
    users: any[] = [];
    roles: any[] = [];
    title: string;
    messageHeader: string;
    message: string;
    selectedUser: string;
    selectedRole: string;
    isError: boolean = false;
    constructor(
        public activeModal: NgbActiveModal,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private securityService: SecurityService,
        private projectUserService: ProjectUserService,
        private dataService: DataService
    ) { }

    ngOnInit() {
        this.title = "Add User to Project";
        this.dataService.currentProject.subscribe(p => this.currentProject = p);
    }

    onUserChange(data) {
        this.selectedUser = data;
    }

    onRoleChange(data) {
        this.selectedRole = data;
    }

    save() {
        Helpers.setLoading(true);
        var mother = this;
        let p = new ProjectUserForAdd(this.currentProject.id, this.selectedUser, this.selectedRole);
        this.projectUserService.AddProjectUser(p).toPromise().then(Response => {
            Helpers.setLoading(false);
            if (Response.result) {
                console.log(Response.result);
                this.isError = false;
                this.activeModal.close();

            }
        }).catch(err => {
            Helpers.setLoading(false);
            this.messageHeader = "Error ";
            this.message = err.error.text;
            this.isError = true;
        });
    }
}