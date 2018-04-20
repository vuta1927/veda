import { Component, Input, OnInit, ViewContainerRef } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../../shared/services/form.service";
import { matchOtherValidator } from "../../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../../shared/utils/form-errors/ngx-errors.service";
import { DxTextBoxModule, DxFormModule } from 'devextreme-angular';
import { Class, ClassForAdd, ClassForUpdate } from '../../../../shared/models/class.model';
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { SecurityService } from '../../../../shared/services/security.service';
import { ClassService } from '../../../services/class.service';
import { Helpers } from '../../../../helpers';
import { DataService } from '../../data.service';
import { ProjectForView } from '../../../../shared/models/project.model';
import { ColorPickerService, Cmyk } from 'ngx-color-picker';
@Component({
    selector: 'app-create-update-class',
    templateUrl: 'create-update-class.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class CreateUpdateClassComponent implements OnInit {
    form: FormGroup;
    currentClass: Class = new Class();
    currentProject: ProjectForView = new ProjectForView("0");
    users: any[] = [];
    roles: any[] = [];
    title: string;
    messageHeader: string;
    message: string;
    selectedUser: string;
    selectedRole: string;
    isError: boolean = false;
    btnSaveDisable: boolean = true;
    isEditMode: boolean = false;
    constructor(
        public activeModal: NgbActiveModal,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private classService: ClassService,
        private dataService: DataService,
        private colorService: ColorPickerService
    ) {

    }

    ngOnInit() {
        this.createForm();
        this.dataService.currentProject.subscribe(p => {
            this.currentProject = p;
        });

        if (this.currentClass.id <= 0) {
            this.title = "Add Class";
            this.isEditMode = false;
        } else {
            this.title = "Edit Class: " + this.currentClass.name;
            this.isEditMode = true;
        }
    }

    createForm() {
        this.form = this.formBuilder.group({
            id: [this.currentClass.id],
            name: [
                this.currentClass.name, [Validators.required],
                this.validateClassNameNotTaken.bind(this)
            ],
            description: [this.currentClass.description],
            code: [
                this.currentClass.code, [Validators.required],
                this.validateCodeNotTaken.bind(this)
            ],
            classColor: [this.currentClass.classColor, [Validators.required]]
        });
        this.ngxErrorsService.setDefaultMessage('nameTaken', { message: 'The class name already taken.' });
        this.ngxErrorsService.setDefaultMessage('codeTaken', { message: 'The  code already taken.' });
    }

    validateClassNameNotTaken(control: AbstractControl) {
        if (this.isEditMode && control.value === this.currentClass.name) {
            this.btnSaveDisable = false;
            return Observable.empty();
        }

        if (!control.value) {
            return Observable.empty();
        }
        let result = null;
        result = this.classService.getClassByName(this.currentProject.id, control.value).toPromise().then(Response => {
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

    validateCodeNotTaken(control: AbstractControl) {
        console.log('code');
        if (this.isEditMode && control.value == this.currentClass.code) {
            this.btnSaveDisable = false;
            return Observable.empty();
        }

        if (!control.value) {
            return Observable.empty();
        }
        let result = null;
        result = this.classService.getCodeOfClass(this.currentProject.id, control.value).toPromise().then(Response => {
            if (Response && Response.result) {
                this.btnSaveDisable = true;
                return { codeTaken: true }
            } else {
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
        if (this.isEditMode) {
            let updateClass = <ClassForUpdate>this.form.value;
            this.classService.UpdateClass(this.currentClass.id, updateClass).toPromise()
                .then(Response => {
                    Helpers.setLoading(false);
                    this.activeModal.close();
                    this.isError = false;
                }).catch(resp => {
                    this.messageHeader = "Error";
                    this.message = resp.result;
                    this.isError = true;
                });
        } else {
            let newClass = <ClassForAdd>this.form.value;
            newClass.projectId = this.currentProject.id;
            this.classService.AddClass(newClass).toPromise().then(Response => {
                Helpers.setLoading(false);
                if (Response.result) {
                    this.activeModal.close();
                    this.isError = false;
                }
            }).catch(resp => {
                this.messageHeader = "Error";
                this.message = resp.result;
                this.isError = true;
            });
        }
    }
}