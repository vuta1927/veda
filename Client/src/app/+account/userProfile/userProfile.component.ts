import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { UserProfileService } from '../../shared/services/userProfile.service';
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl } from "@angular/forms";
import { FormService } from "../../shared/services/form.service";
import { matchOtherValidator } from "../../shared/validators/validators";
import { IUserProfile, UserProfile } from '../../shared/models/user.model';
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../shared/utils/form-errors/ngx-errors.service";
import { Observable } from "rxjs/Observable";
import swal from "sweetalert2";
@Component({
    selector: 'app-user-projfile',
    templateUrl: './userProfile.component.html',
    styleUrls: ['./userProfile.component.css'],
    encapsulation: ViewEncapsulation.None
})

export class UserProfileComponent implements OnInit {
    userProfile: UserProfile = new UserProfile();
    form: FormGroup;
    constructor(
        private userProfileService: UserProfileService,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService
    ) {
    }
    ngOnInit() {
        this.createForm();
        this.ngxErrorsService.setDefaultMessage('usernameTaken', { message: 'The username already taken.' });
        this.ngxErrorsService.setDefaultMessage('emailTaken', { message: 'The email already taken.' })
        this.userProfileService.getProject().toPromise().then(Response => {
            if (Response.result) {
                this.userProfile = Response.result;
                this.userProfileService.changeUserProfile(Response.result);
            }

        }).catch(Response => {
            swal({ text: Response.error ? Response.error.text : Response.message, type: "error" })
        })
    
    }

    createForm() {
        this.form = this.formBuilder.group(
            {
                id: [
                    this.userProfile.id
                ],
                name: [
                    this.userProfile.name,
                    [Validators.required, Validators.maxLength(20)]
                ],
                surname: [
                    this.userProfile.surname,
                    [Validators.required, Validators.maxLength(20)]
                ],
                email: [
                    this.userProfile.email,
                    [Validators.required, Validators.email, Validators.maxLength(50)],
                    this.validateEmailNotTaken.bind(this)
                ],
                username: [
                    this.userProfile.username,
                    [Validators.required, Validators.maxLength(50)],
                    this.validateUsernameNotTaken.bind(this)
                ],
            });
    }

    validateUsernameNotTaken(control: AbstractControl) {
        if (control.value === this.userProfile.username) {
            return Observable.empty();
        }
        return this.userProfileService.getByUsername(control.value).map(res => {
            return res.result ? { usernameTaken: true } : null;
        }).catch(_ => Observable.empty());
    }

    validateEmailNotTaken(control: AbstractControl) {
        if (control.value === this.userProfile.email) {
            return Observable.empty();
        }
        return this.userProfileService.getByEmail(control.value).map(res => {
            return res.result ? null : { emailTaken: true };
        }).catch(_ => Observable.empty());
    }
    saveChange(){}
}