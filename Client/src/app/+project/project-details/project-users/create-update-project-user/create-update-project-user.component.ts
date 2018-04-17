import { Component, Input, OnInit } from "@angular/core";
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
@Component({
    selector: 'create-update-project-user',
    templateUrl: './create-update-project-user.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class CreateUpdateProjectUserComponent implements OnInit {
    form: FormGroup;
    projectUser: any = null;
    users: any[] = [];
    roles: any[] = [];
    title: string;
    messageHeader: string;
    message: string;
    btnSaveDisable: boolean = true;

    constructor(
        public activeModal: NgbActiveModal,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private securityService: SecurityService,
        private projectUserService: ProjectUserService
    ) { }

    ngOnInit() {
        // this.createForm();
        this.title = "Add User to Project";
    }

    createForm() {
        this.form = this.formBuilder.group({
            userName: [this.projectUser.userName, [Validators.required]],
            roleName: [this.projectUser.roleName]
        });
    }

    save() {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }
        Helpers.setLoading(true);

        console.log(this.form.value);
        let p = <ProjectUserForAdd>this.form.value;
        p.id = "0";
        this.projectUserService.AddProjectUser(p).toPromise().then(Response => {
            Helpers.setLoading(false);
            if (Response.result) {
                this.activeModal.close();
            }
        }).catch(err => {
            this.messageHeader = "Error";
            this.message = err.result;
            $('#errorMessage').css("display", "block");
        });
    }
}