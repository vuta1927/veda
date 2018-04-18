import { Component, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef } from '@angular/core';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../helpers';
import { PermissionCategory } from '../shared/models/permission.model';
import { ProjectService } from './project.service';
import { CreateUpdateProjectComponent } from './create-update/create-update-project.component';
import { ProjectForAdd } from '../shared/models/project.model';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../shared/services/security.service';
@Component({
    selector: 'app-project',
    templateUrl: './project.component.html',
    styleUrls: [
        './project.component.css'
    ],
    encapsulation: ViewEncapsulation.None
})
export class ProjectComponent {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    dataSource: any = {};
    permissionSource: any;
    selectedProjects: any[] = [];
    permissions: PermissionCategory[];
    isAdmin: boolean = false;
    noData: string = "There are no data to load";
    constructor(private modalService: NgbModal, private securityService: SecurityService, private projectService: ProjectService, public toastr: ToastsManager, private vcr: ViewContainerRef) {
        this.toastr.setRootViewContainerRef(vcr);
        let currentUserData = this.securityService.getUserRoles();
        if(currentUserData == "Administrator")
            this.isAdmin = true;
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                return projectService.getProject()
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

    selectionChanged(data: any) {
        this.selectedProjects = data.selectedRowsData;
    }

    deleteSelectedProject() {
        Helpers.setLoading(true);
        let ids = '';
        for (let i = 0; i < this.selectedProjects.length; i++) {
            if (i != (this.selectedProjects.length - 1))
                ids += this.selectedProjects[i].id + '_';
            else
                ids += this.selectedProjects[i].id;
        }

        // console.log(ids);
        var mother = this;
        this.projectService.DeleteProject(ids).toPromise().then(Response => {
            Helpers.setLoading(false);
            mother.dataGrid["first"].instance.refresh();
            if (Response && Response.result) {
                let error = Response.result.split('#')[1];
                mother.showError(error);
            }else{
                mother.showInfo("Projects deleted");
            }
        });
    }

    cellNameClicked(data) {
        console.log(data);
    }

    addProject() {
        this.openCreateOrUpdateModal();
    }

    openCreateOrUpdateModal(project?) {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        const modalRef = this.modalService.open(CreateUpdateProjectComponent, config);
        if (project)
            modalRef.componentInstance.project = project;
        else {
            modalRef.componentInstance.project = new ProjectForAdd(null, null);
        }
        var mother = this;
        modalRef.result.then(function () {
            mother.dataGrid["first"].instance.refresh();
        })
    }

    showSuccess(message: string) {
        this.toastr.success(message, 'Success!');
    }

    showError(message: string) {
        this.toastr.error(message, 'Oops!');
    }

    showInfo(message: string) {
        this.toastr.info(message);
    }
}