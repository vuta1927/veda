import { Component, Input, OnInit } from "@angular/core";
import { RolesService } from '../roles.service';
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../shared/services/form.service";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { Role, RoleForCreate, RoleForView, RoleForUpdate } from "../../../shared/models/role.model";
import { SecurityService } from '../../../shared/services/security.service';
import { UsersService } from '../../users/users.service';
declare let mApp: any;
@Component({
    selector: 'create-or-update-role',
    templateUrl: './create-or-update-role.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class CreateOrUpdateRoleComponent implements OnInit {
    form: FormGroup;
    role: any;
    title: string;
    messageHeader: string;
    message: string;
    isEditMode: boolean;
    constructor(public activeModal: NgbActiveModal, private formBuilder: FormBuilder, public formService: FormService, private ngxErrorsService: NgxErrorsService, private securityService: SecurityService, private usersService: UsersService, private rolesService: RolesService) { 
        
    }

    ngOnInit() {
        if (this.role) {
            this.isEditMode = true;
            this.title = "Edit Role: " + this.role.roleName;
        } else {
            this.role = new RoleForCreate(-1, '', '', '');
            this.isEditMode = false;
            this.title = "Add Role";
        }
        this.createForm();
    }

    createForm() {
        this.form = this.formBuilder.group({
            id: new FormControl(this.role.id),
            roleName: new FormControl(this.role.roleName, [Validators.required]),
            descriptions: new FormControl(this.role.descriptions)
        });
    }

    save() {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }
        if (this.isEditMode) {
            let role = <RoleForUpdate>this.form.value;
            var id = this.securityService.getUserId();
            role.lastModifierUserId = Number(id);
            this.rolesService.UpdateRole(role).toPromise()
            .then(Response =>{
                if(Response.result){
                    this.activeModal.close();
                }else{
                    this.messageHeader = "Role Exist";
                    this.message = "Role is exist, please try another name!";
                    $('#errorMessage').css("display", "block");
                }
            });
        }else{
            let role = <RoleForCreate>this.form.value;
            var id = this.securityService.getUserId();
            role.creatorUserId = Number(id);
            this.rolesService.AddRole(role).toPromise().then(Response=>{
                if(Response.result){
                    this.activeModal.close();
                }else{
                    this.messageHeader = "Role Exist";
                    this.message = "Role is exist, please try another name!";
                    $('#errorMessage').css("display", "block");
                }
            });
        }
    }
}