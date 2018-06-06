import { Component, OnInit, Input, ViewContainerRef } from '@angular/core';
import { Router } from '@angular/router';
import { ProjectForView } from '../../../shared/models/project.model';
import { FormControl, FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { ProjectForUpdate } from '../../../shared/models/project.model';
import { FormService } from "../../../shared/services/form.service";
import { DataService } from '../data.service';
import { ProjectService } from '../../project.service';
import { Constants } from '../../../constants';
import { SecurityService } from '../../../shared/services/security.service';
import { Observable } from "rxjs/Observable";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { Helpers } from '../../../helpers';
import * as _ from 'lodash';
@Component({
  selector: 'app-project-info',
  templateUrl: './project-info.component.html',
  styleUrls: ['./project-info.component.css'],
  providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})
export class ProjectInfoComponent implements OnInit {
  currentProject: ProjectForView = new ProjectForView();
  btnSaveDisable: boolean = true;
  form: FormGroup;
  messageHeader: string;
  message: string;
  viewProject: boolean = false;
  editProject: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastsManager,
    private vcr: ViewContainerRef,
    private dataService: DataService,
    private projectService: ProjectService,
    private ngxErrorsService: NgxErrorsService,
    public formService: FormService,
    private securityService: SecurityService,
    private router: Router
  ) {
    this.toastr.setRootViewContainerRef(vcr);
  }

  ngOnInit() {
    this.viewProject = this.securityService.IsGranted(Constants.viewProject);
    this.editProject = this.securityService.IsGranted(Constants.editProject);
    this.dataService.currentProject.subscribe(p => {
      this.currentProject = p;
      this.createForm();
    }, error => {
      console.log(error)
    });
  }

  createForm() {
    this.form = this.formBuilder.group({
      id: [this.currentProject.id],
      name: [this.currentProject.name, [Validators.required], this.validateProjectNameNotTaken.bind(this)],
      description: [this.currentProject.description],
      note: [this.currentProject.note]
    });
    this.ngxErrorsService.setDefaultMessage('nameTaken', { message: 'The project name already taken.' });
  }

  validateProjectNameNotTaken(control: AbstractControl) {
    if (control.value === this.currentProject.name) {
      return Observable.empty();
    }

    if (!control.value) {
      return Observable.empty();
    }
    let result = null;
    result = this.projectService.getProjectByName(control.value).toPromise().then(Response => {
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

  onValueChange(value: string) {
    this.btnSaveDisable = false;
  }

  cancel() {
    this.createForm();
    this.btnSaveDisable = true;
  }

  saveChange() {
    if (this.form.invalid) {
      this.formService.validateAllFormFields(this.form);
      return;
    }
    // console.log(this.currentProject);
    let proj = <ProjectForUpdate>this.form.value;
    Helpers.setLoading(true);

    let project = <ProjectForUpdate>this.form.value;
    this.projectService.UpdateProject(project).toPromise()
      .then(Response => {
        Helpers.setLoading(false);
        if (Response.result) {
          this.showSuccess("Project updated !");
          this.btnSaveDisable = true;
        } else {
          this.messageHeader = "Error";
          this.message = "Cant Update Project!";
          $('#errorMessage').css("display", "block");
        }
      });

  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success!', { toastLife: 1600, showCloseButton: true });
  }

  showError(message: string) {
    this.toastr.error(message, 'Oops!', { toastLife: 1600, showCloseButton: true });
  }

  showInfo(message: string) {
    this.toastr.info(message, null, { toastLife: 1600, showCloseButton: true });
  }
}