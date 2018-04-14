import { Component, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef, OnInit } from '@angular/core';
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { PermissionCategory } from '../../shared/models/permission.model';
import { ProjectService } from '../project.service';
import { CreateUpdateProjectComponent } from './create-update/create-update-project.component';
import { ProjectForAdd } from '../../shared/models/project.model';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../../shared/services/security.service';
import { ActivatedRoute } from '@angular/router';
import { DataService } from './data.service';
import 'rxjs/add/operator/filter';

@Component({
    selector: 'app-project-details',
    templateUrl: './project-details.component.html',
    styleUrls: [
        './project-details.component.css'
    ],
    encapsulation: ViewEncapsulation.None
})
export class ProjectDetailsComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    projectId: string = '';
    currentProject: any = {};
    dataSource: any = {};
    permissionSource: any;
    permissions: PermissionCategory[];
    isAdmin: boolean = false;
    constructor(
        private modalService: NgbModal, 
        private securityService: SecurityService, 
        private projectService: ProjectService, 
        public toastr: ToastsManager, 
        private vcr: ViewContainerRef,
        private route: ActivatedRoute,
        private dataSerivce: DataService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        let currentUserData = this.securityService.getUserRoles();
        if (currentUserData == "Administrator")
            this.isAdmin = true;

    }

    ngOnInit() {
        this.route.queryParams.filter(params => params.id).subscribe(params => {
            this.projectId = params.id;
            this.projectService.getProjectById(this.projectId).toPromise().then(Response=>{
                if(Response && Response.result){
                    this.currentProject = Response.result;
                    
                    this.dataSerivce.changeProject(this.currentProject);
                }
            });
        });
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