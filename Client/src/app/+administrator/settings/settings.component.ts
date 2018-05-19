import { Component, OnInit } from '@angular/core';
import { ProjectSetting, ProjectSettingForUpdate } from '../../shared/models/project-setting.model';
import { ProjectSettingService } from './settings.service';
import { Helpers } from '../../helpers';
import { FormGroup, FormBuilder, Validators, ValidationErrors, AbstractControl, FormControl } from "@angular/forms";
import { FormService } from "../../shared/services/form.service";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../shared/utils/form-errors/ngx-errors.service";
import swal from 'sweetalert2';
import { DISABLED } from '@angular/forms/src/model';
import responsive_box from 'devextreme/ui/responsive_box';

@Component({
    selector: 'app-settings',
    styleUrls: ['settings.component.css'],
    templateUrl: 'settings.component.html',
})

export class SettingsComponent implements OnInit{
    form: FormGroup;
    projects = [];
    selectedProject: any;
    btnSaveDisable = true;
    projectSetting: ProjectSettingForUpdate = new ProjectSettingForUpdate();
    constructor(
        private settingService: ProjectSettingService,
        private formBuilder: FormBuilder,
        public formService: FormService,
        private ngxErrorsService: NgxErrorsService,
    ){
        this.createForm();

        this.settingService.getProjects().toPromise().then(Response=>{
            if(Response.result){
                this.projects = Response.result;
                this.selectedProject = this.projects[0];
                this.getSetting();
            }
        }).catch(Response=>{
            swal({
                title:'', 
                text:Response.error? Response.error.text:Response.message,
                type:'error'
            });
        })
    }


    ngOnInit(){}

    createForm() {
        this.form = this.formBuilder.group({
            taggTimeValue: new FormControl({ value: this.projectSetting.quantityCheckLevel, DISABLED: this.selectedProject? true: false}, Validators.required),
            quantityCheckLevel: new FormControl({value: this.projectSetting.quantityCheckLevel, DISABLED: this.selectedProject? true: false}, Validators.required)
        });
    }

    getSetting(){
        this.btnSaveDisable = true;
        Helpers.setLoading(true);
        this.settingService.getSetting(this.selectedProject.id).toPromise().then(Response=>{
            Helpers.setLoading(false);
            if(Response.result){
                this.btnSaveDisable = false;
                this.projectSetting.projectId = this.selectedProject.id;
                this.projectSetting.quantityCheckLevel = Response.result.quantityCheckLevel;
                this.projectSetting.taggTimeValue = Response.result.taggTimeValue;
            }
        }).catch(Response=>{
            Helpers.setLoading(false);
            this.btnSaveDisable = true;
            swal({
                title:'', 
                text:Response.error? Response.error.text:Response.message,
                type:'error'
            });
        })
    }

    projectChanged(target:any){
        this.selectedProject = this.projects.find(x=>x.id == target.value);
        this.getSetting();
    }

    save(){
        if (this.form.invalid) {
            this.formService.validateAllFormFields(this.form);
            return;
        }
        Helpers.setLoading(true);
        let data = <ProjectSettingForUpdate>this.form.value;
        data.projectId = this.selectedProject.id;

        this.settingService.Update(data.projectId, data).toPromise().then(Response=>{
            swal({
                title:'', 
                text:'Project setting changed !',
                type:'success'
            }).then(()=>{
                Helpers.setLoading(false);
            });
        }).catch(Response=>{
            swal({
                title:'', 
                text:Response.error? Response.error.text:Response.message,
                type:'error'
            }).then(()=>{
                Helpers.setLoading(false);
            });
        })
    }

    cancel(){
        this.getSetting();
    }
}