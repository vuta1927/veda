import { Component, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef } from '@angular/core';
import {Router, ActivatedRoute} from '@angular/router';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../helpers';
import { ProjectService } from './project.service';
import { CreateUpdateProjectComponent } from './create-update/create-update-project.component';
import { ProjectForAdd } from '../shared/models/project.model';
import { SecurityService } from '../shared/services/security.service';
import { Constants } from '../constants';
import swal from 'sweetalert2';
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
    selectedProjects: any[] = [];
    isAdmin: boolean = false;
    addProject: boolean = false;
    editProject: boolean = false;
    deleteProject: boolean = false;

    noData: string = "There are no data to load";
    constructor(
        private modalService: NgbModal, 
        private securityService: SecurityService, 
        private projectService: ProjectService, 
        private vcr: ViewContainerRef,
        private route: Router,
        private acRoute: ActivatedRoute
    ) {
        this.isAdmin = this.securityService.IsGranted(Constants.admin);
        this.addProject = this.securityService.IsGranted(Constants.addProject);
        this.editProject = this.securityService.IsGranted(Constants.editProject);
        this.deleteProject = this.securityService.IsGranted(Constants.deleteProject);
        
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                var params = '';

                params += loadOptions.skip || 0;
                params += '/';
                params += loadOptions.take || 12;

                return projectService.getProject(params)
                    .toPromise().then(response => {
                        return projectService.getTotal().toPromise().then(resp => {
                            if (resp.result) {
                                return {
                                    data: response.result,
                                    totalCount: resp.result,
                                }
                            }else{
                                return{
                                    data: response.result,
                                    totalCount: 0
                                }
                            }
                        })

                    }).catch(error => { 
                        if(error.status == 401 || error.status == 403){
                            this.route.navigate(['#']);
                            return;
                        };
                        console.log(error);
                        throw 'Data Loading Error';
                     });
            }
        });
    }

    selectionChanged(data: any) {
        this.selectedProjects = data.selectedRowsData;
    }

    // importClicked(){
    // }

    // exportClicked(){
    // }

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
                swal({text:error, type: 'error'});
            } else {
                swal({text:"Projects deleted", type:'success'});
            }
        });
    }

    mergeProject(){
        var params = [];
        this.selectedProjects.forEach(p => {
            params.push(p.id);
        });
        this.route.navigate(['merge-project', { projects: params }]);
    }

    cellNameClicked(data) {
        // console.log(data);
    }

    addProjectClicked() {
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
}