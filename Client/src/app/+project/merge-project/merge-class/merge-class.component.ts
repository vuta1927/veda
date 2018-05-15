import { Component, Input, OnInit, ViewContainerRef } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../../shared/services/form.service";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { DxTextBoxModule, DxFormModule } from 'devextreme-angular';
import { Class, ClassForAdd, ClassForUpdate } from '../../../shared/models/class.model';
import { Observable } from "rxjs/Observable";
import * as _ from 'lodash';
import { SecurityService } from '../../../shared/services/security.service';
import { ClassService } from '../../services/class.service';
import { Helpers } from '../../../helpers';
import { ProjectForView } from '../../../shared/models/project.model';
import { ColorPickerService, Cmyk } from 'ngx-color-picker';
import { DataService } from '../../data.service';
@Component({
    selector: 'app-merge-class',
    templateUrl: 'merge-class.component.html',
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class MergeClass implements OnInit {
    form: FormGroup;
    messageHeader: string;
    message: string;
    isError: boolean = false;
    btnSaveDisable: boolean = true;
    classes: Class[];
    mergeClasses: Class[];
    newClass: Class;
    classColor: string = '#000000';
    projectName: string;
    constructor(
        public activeModal: NgbActiveModal,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
        private classService: ClassService,
        private colorService: ColorPickerService,
        private dataService: DataService
    ) {

    }

    ngOnInit() {
        this.createForm();
    }

    createForm() {
        this.form = this.formBuilder.group({
            name: [
                '', [Validators.required],
                this.validateClassNameNotTaken.bind(this)
            ],
            description: [''],
            classColor: ['#ffffff', [Validators.required]]
        });
        this.ngxErrorsService.setDefaultMessage('nameTaken', { message: 'The class name already taken.' });
    }

    validateClassNameNotTaken(control: AbstractControl) {
        if (!control.value) {
            return Observable.empty();
        }

        let classesNotMerge = [];
        this.classes.forEach(klass => {
            if(!this.mergeClasses.find(x=>x.name == klass.name)){
                classesNotMerge.push(klass);
            }
        });

        if(classesNotMerge.find(x=>x.name == control.value)){
            this.btnSaveDisable = true;
            return { nameTaken: true };
        }else{
            this.btnSaveDisable = false;
            return Observable.empty();
        }
    }


    save() {
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }
        Helpers.setLoading(true);
        let totalTag = 0;
        this.mergeClasses.forEach(e => {
            var klass = this.classes.find(x=>x.name == e.name);
            if(klass){
                this.classes.splice(this.classes.indexOf(klass), 1);
            }
            totalTag += e.totalTag;

        });

        this.newClass = <Class>this.form.value;
        this.newClass.id = 0;
        this.newClass.project = this.projectName;
        this.newClass.totalTag = totalTag;
        this.classes.push(this.newClass);

        this.dataService.changeNewClass(this.newClass);
        this.dataService.changeClass(this.classes);

        Helpers.setLoading(false);
        this.activeModal.dismiss();
        this.activeModal.close();
    }
}