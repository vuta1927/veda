import { Component, Input, OnInit } from "@angular/core";
import { PermissionService } from './permission.service';
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../shared/services/form.service";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { DxCheckBoxModule, DxSelectBoxModule, DxFormModule } from 'devextreme-angular';

import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { PermissionForView, PermissionCategory } from "../../../shared/models/permission.model";
import { SecurityService } from '../../../shared/services/security.service';
import { UsersService } from '../../users/users.service';
declare let mApp: any;
@Component({
    selector: 'add-permission-to-role',
    templateUrl: './add-permission-to-role.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class AddPermissionToRoleComponent implements OnInit {
    form: FormGroup;
    permissions: PermissionCategory[];
    permissionCategory: any;
    role: any;
    title: string;
    messageHeader: string;
    message: string;
    isEditMode: boolean;
    currentCategory: string;
    constructor(public activeModal: NgbActiveModal, private formBuilder: FormBuilder, public formService: FormService, private ngxErrorsService: NgxErrorsService, private securityService: SecurityService, private usersService: UsersService, private permissionService: PermissionService) { }

    ngOnInit() {
        this.createForm();
    }

    createForm() {
        this.permissions = new Array<PermissionCategory>();
        this.permissionService.getCategory().toPromise().then(Response => {
            if (Response.result) {
                this.permissionCategory = Response.result;
            }
        });

    }
    valueChanged(data) {
        // console.log(data.value);
        if (data.value && this.role) {
            this.currentCategory = data.value;
            this.permissionService.getPermissionCategory(data.value, this.role.id).toPromise().then(Response => {
                if(Response.result){
                    Response.result.forEach(p => {
                        this.permissions.push(new PermissionCategory(p.id, p.name, p.descriptions, p.category, p.displayName, p.isCheck));
                    });
                }
            });
        }

    }

    onCheckboxChecked(data){
        var id = Number(data.element.id);
        this.permissions.forEach(permission => {
            if (permission.id == id){
                permission.isCheck = data.value;
            }
        });
        console.log(this.permissions);
    }

    save(){
        console.log(this.currentCategory);
    }
}