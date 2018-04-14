import { Component, Input, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../shared/services/form.service";
import { matchOtherValidator } from "../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../shared/utils/form-errors/ngx-errors.service";
import { DxTextBoxModule, DxFormModule } from 'devextreme-angular';
import { ProjectForAdd, ProjectForUpdate } from '../../shared/models/project.model';
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { SecurityService } from '../../shared/services/security.service';
import { ProjectService } from '../project.service';
import { Helpers } from '../../helpers';
@Component({
    selector: 'create-update-project',
    templateUrl: './create-update-project.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class CreateUpdateProjectComponent implements OnInit {
    form: FormGroup;
    project: any = null;
    title: string;
    messageHeader: string;
    message: string;
    isEditMode: boolean;
    currentCategory: string;
    btnSaveDisable: boolean = true;

    constructor(
        public activeModal: NgbActiveModal,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private securityService: SecurityService,
        private projectService: ProjectService
    ) { }

    ngOnInit() {
        this.createForm();

        if (this.project.id <= 0) {
            this.title = "Add Project";
            this.isEditMode = false;
        } else {
            this.title = "Edit Project: " + this.project.name;
            this.isEditMode = true;
        }
    }

    createForm() {
        this.form = this.formBuilder.group({
            id: [this.project.id],
            name: [this.project.name, [Validators.required], this.validateProjectNameNotTaken.bind(this)],
            description: [this.project.description],
            note: [this.project.note]
        });
        this.ngxErrorsService.setDefaultMessage('nameTaken', { message: 'The project name already taken.' });
    }

    validateProjectNameNotTaken(control: AbstractControl) {
        console.log("a");
        if (this.isEditMode && control.value === this.project.name) {
            this.btnSaveDisable = false;
            return Observable.empty();
        }

        if (!control.value){
            return Observable.empty();
        }
        let result = null;
        result = this.projectService.getProject(control.value).toPromise().then(Response => {
            if (Response && Response.result) {
                this.btnSaveDisable = true;
                return { nameTaken: true }
            } else
                {
                    this.btnSaveDisable = false;
                    return null;
                }
        });
        return result;
    }

    save() {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }
        Helpers.setLoading(true);

        console.log(this.form.value);
        if (this.isEditMode) {
            let project = <ProjectForUpdate>this.form.value;
            this.projectService.UpdateProject(project).toPromise()
                .then(Response => {
                    Helpers.setLoading(false);
                    if (Response.result) {
                        this.activeModal.close();
                    } else {
                        this.messageHeader = "Error";
                        this.message = "Cant Update Project!";
                        $('#errorMessage').css("display", "block");
                    }
                });
        } else {
            let project = <ProjectForAdd>this.form.value;
            this.projectService.AddProject(project).toPromise().then(Response => {
                Helpers.setLoading(false);
                if (Response.result) {
                    this.activeModal.close();
                } else {
                    this.messageHeader = "Error";
                    this.message = "Can't create Project!";
                    $('#errorMessage').css("display", "block");
                }
            });
        }

        console.log(this.currentCategory);
    }
}