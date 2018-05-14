import { Component, Input, OnInit, ViewContainerRef } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../shared/services/form.service";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { DxTextBoxModule, DxFormModule } from 'devextreme-angular';
import { ProjectUser } from '../../../shared/models/project-user.model';
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { SecurityService } from '../../../shared/services/security.service';
import { ProjectUserService } from '../../services/project-users.service';
import { Helpers } from '../../../helpers';
import { DataService } from '../../data.service';
import { RolesService } from '../../../+administrator/roles/roles.service';
@Component({
    selector: 'update-user',
    templateUrl: './update-user.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class UpdateUserComponent implements OnInit {
    form: FormGroup;
    projectUser: any;
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
        private dataService: DataService,
        private roleService: RolesService
    ) { }

    ngOnInit() {
        var mother = this;
        Helpers.setLoading(true);
        this.roleService.getProjectRoles().toPromise().then(res => {
            if (res && res.result) {
                mother.roles = res.result;
                Helpers.setLoading(false);
            }
        })
    }

    onUserChange(data) {
        this.selectedUser = data;
    }

    onRoleChange(data) {
        this.selectedRole = data;
    }

    save() {
        Helpers.setLoading(true);
        this.projectUser.roleName = this.selectedRole;

        this.dataService.changeProjectUser(this.projectUser);
        Helpers.setLoading(false);
        this.activeModal.close();
    }
}