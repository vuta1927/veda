import { Component, ViewEncapsulation, ViewChildren, OnInit, Input, ViewContainerRef } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { Tag } from '../../../shared/models/tag.model';
import { ProjectForView } from '../../../shared/models/project.model';
import { FormService } from "../../../shared/services/form.service";
import { DataService } from '../data.service';
import { ClassService } from '../../services/class.service';
import { Observable } from "rxjs/Observable";
import { matchOtherValidator } from "../../../shared/validators/validators";
import { NGX_ERRORS_SERVICE_CHILD_PROVIDERS, NgxErrorsService } from "../../../shared/utils/form-errors/ngx-errors.service";
import { IAppCoreResponse } from "../../../shared/models/appcore-response.model";
import { Helpers } from '../../../helpers';
import * as _ from 'lodash';

import { ConfigurationService } from "../../../shared/services/configuration.service";

import { DxDataGridComponent } from 'devextreme-angular';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { CreateUpdateClassComponent } from './create-update/create-update-class.component';
@Component({
    selector: 'app-project-class',
    templateUrl: 'project-class.component.html',
    styleUrls: ['project-class.component.css'],
    encapsulation: ViewEncapsulation.None,
    providers: [NGX_ERRORS_SERVICE_CHILD_PROVIDERS]
})

export class ProjectClassComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent;
    dataSource: any = {};
    selectedClasses: any[] = [];
    currentProject: ProjectForView = new ProjectForView("0");
    form: FormGroup;
    messageHeader: string;
    message: string;
    apiUrl: string = '';
    constructor(
        private modalService: NgbModal,
        private formBuilder: FormBuilder,
        private toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private dataService: DataService,
        private classService: ClassService,
        private ngxErrorsService: NgxErrorsService,
        public formService: FormService,
        private configurationService: ConfigurationService,
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        let mother = this;
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                return this.classService.getClasses(mother.currentProject.id)
                    .toPromise()
                    .then(response => {

                        return {
                            data: response.result,
                            totalCount: response.result.length
                        }
                    })
                    .catch(error => { throw 'Data Loading Error' });
            }
        });
    }

    ngOnInit() {
        this.dataService.currentProject.subscribe(p => {
            this.currentProject = p;
            this.classService.getClasses(p.id).toPromise().then(Response => {
                if (Response && Response.result) {
                    this.dataSource = Response.result;
                    this.dataGrid["first"].instance.refresh();
                }
            });
        }, error => {
            console.log(error)
        });
    }

    selectionChanged(data: any) {
        this.selectedClasses = data.selectedRowsData;
    }

    deleteSelectedImages() {
        Helpers.setLoading(true);
        let ids = '';
        console.log(this.selectedClasses);
        for (let i = 0; i < this.selectedClasses.length; i++) {
            if (i != (this.selectedClasses.length - 1))
                ids += this.selectedClasses[i].id + '_';
            else
                ids += this.selectedClasses[i].id;
        }

        // console.log(ids);
        var mother = this;
        this.classService.DeleteClass(ids).toPromise().then(Response => {
            Helpers.setLoading(false);
            mother.dataGrid["first"].instance.refresh();
            if (Response && Response.result) {
                mother.showInfo("class deleted");
            }
        }).catch(res => {
            let error = res.result;
            mother.showError(error);
        });
    }

    addClass(data){
        this.openCreateOrUpdateModal();
    }

    classSelected(data) {
        this.openCreateOrUpdateModal(data);
    }

    openCreateOrUpdateModal(classSelected?) {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        const modalRef = this.modalService.open(CreateUpdateClassComponent, config);
        if (classSelected)
            modalRef.componentInstance.currentClass = classSelected;
        var mother = this;
        modalRef.result.then(function () {
            mother.dataGrid["first"].instance.refresh();
        })
    }

    showSuccess(message: string) {
        this.toastr.success(message, 'Success!', { toastLife: 2000, showCloseButton: true });
    }

    showError(message: string) {
        this.toastr.error(message, 'Oops!', { toastLife: 3000, showCloseButton: true });
    }

    showInfo(message: string) {
        this.toastr.info(message, null, { toastLife: 3000, showCloseButton: true });
    }
}