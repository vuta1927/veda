import { Component, Input, OnInit } from "@angular/core";
import { UsersService } from "./users.service";
import { IUserForCreateOrEdit, CreateOrUpdateUser, IUserEdit } from "../../shared/models/user.model";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl } from "@angular/forms";
import { FormService } from "../../shared/services/form.service";
import { matchOtherValidator } from "../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../shared/utils/form-errors/ngx-errors.service";
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';

declare let mApp: any;
@Component({
    selector: 'create-or-update-user',
    templateUrl: './create-or-update-user.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class CreateOrUpdateUserComponent implements OnInit {

    userForCreateOrEdit: IUserForCreateOrEdit;

    form: FormGroup;

    constructor(
        public activeModal: NgbActiveModal,
        private userService: UsersService,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService
    ) {
        
    }

    ngOnInit() {
        this.createForm();
        this.assignedRoleNames = this.userForCreateOrEdit.roles.filter(r => r.isAssigned).map(r => { return r.roleName; });
        this.isSetRandomPassword = !this.userForCreateOrEdit.isEditMode;
        this.ngxErrorsService.setDefaultMessage('usernameTaken', { message: 'The username already taken.'});
        this.ngxErrorsService.setDefaultMessage('emailTaken', { message: 'The email already taken.'})
    }

    createForm() {
        this.form = this.formBuilder.group(
            {
                id: [
                    this.userForCreateOrEdit.user.id
                ],
                name: [
                    this.userForCreateOrEdit.user.name,
                    [Validators.required, Validators.maxLength(20)]
                ],
                surname: [
                    this.userForCreateOrEdit.user.surname ,
                    [Validators.required, Validators.maxLength(20)]
                ],
                emailAddress: [
                    this.userForCreateOrEdit.user.emailAddress,
                    [Validators.required, Validators.email, Validators.maxLength(50)],
                    this.validateEmailNotTaken.bind(this)
                ],
                username: [
                    this.userForCreateOrEdit.user.username,
                    [Validators.required, Validators.maxLength(50)],
                    this.validateUsernameNotTaken.bind(this)
                ],
                password: null,
                passwordRepeat: null,
                shouldChangePasswordOnNextLogin: this.userForCreateOrEdit.user.shouldChangePasswordOnNextLogin,
                sendActivationEmail: true,
                isActive: this.userForCreateOrEdit.user.isActive
            });
    }

    validateUsernameNotTaken(control: AbstractControl) {
        if (this.userForCreateOrEdit.isEditMode && control.value === this.userForCreateOrEdit.user.username) {
            return Observable.empty();
        }
        return this.userService.getByUsername(control.value).map(res => {
            return res.result ? { usernameTaken: true } : null;
        }).catch(_ => Observable.empty());
    }

    validateEmailNotTaken(control: AbstractControl) {
        if (this.userForCreateOrEdit.isEditMode && control.value === this.userForCreateOrEdit.user.emailAddress) {
            return Observable.empty();
        }
        return this.userService.getByEmail(control.value).map(res => {
            return res.result ? null : { emailTaken: true };
        }).catch(_ => Observable.empty());
    }

    private isSetRandomPassword: boolean;

    get setRandomPassword(): boolean {
        return this.isSetRandomPassword;
    }

    set setRandomPassword(value: boolean) {
        if (value === true) {
            this.form.get('password').clearValidators();
            this.form.get('passwordRepeat').clearValidators();
        } else {
            this.form.get('password').setValidators([Validators.required, Validators.minLength(6), Validators.maxLength(50)])
            this.form.get('passwordRepeat').setValidators([Validators.required, Validators.minLength(6), Validators.maxLength(50), matchOtherValidator('password')]);
        }
        console.log(value);
        this.form.get('password').reset();
        this.form.get('passwordRepeat').reset();
        this.isSetRandomPassword = value;
    }

    private assignedRoleNames: string[];
    private roleSelectedChange(data) {
        if (data.target.checked === true) {
            this.assignedRoleNames.push(data.target.value);
        } else {
            _.remove(this.assignedRoleNames, function(n) {
                return n === data.target.value;
            });
        }
    }

    save() {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }

        let user = <IUserEdit>this.form.value;
        // let userCreateOrEdit = new CreateOrUpdateUser();
        // userCreateOrEdit.user = user;
        user["sendActivationEmail"] = this.form.get('sendActivationEmail').value;
        user["assignedRoleNames"] = this.assignedRoleNames;

        if(!this.userForCreateOrEdit.isEditMode){
            this.userService.AddUser(user).toPromise().then(Response=>{
                if(Response.result){
                    // console.log(Response.result);
                    this.activeModal.close();
                }
            });
        }else{
            this.userService.UpdateUser(user).toPromise().then(Response=>{
                if(Response.result){
                    this.activeModal.close();
                }
            });
        }

        console.log(user);
    }
}