import { Component } from '@angular/core';
import { SecurityService } from '../../shared/services/security.service';
import { OAuthService } from 'angular-oauth2-oidc';
import { Chart } from 'chart.js';
import { DashBoardService } from './analytics.service';
import swal from 'sweetalert2';
import responsive_box from 'devextreme/ui/responsive_box';
import { projectAnalist } from '../../shared/models/dashboard.model';
import { Data, Datasets } from '../../shared/models/chart.model';
@Component({
    selector: 'app-analytics',
    templateUrl: './analytics.component.html'
})
export class AnalyticsComponent {
    imageTaggedChart = [];
    projects = [];
    selectedProject: any;
    currentProjectData = new projectAnalist();
    constructor(
        private dashboardSerive: DashBoardService
    ) {
        this.dashboardSerive.getProjectsByUser().toPromise().then(Response => {
            if (Response.result) {
                this.projects = Response.result;
                this.selectedProject = Response.result[0];
                this.getProjectData(Response.result[0].id);
            }
        }).catch(Response => {
            swal({ text: Response.error ? Response.error.text : Response.message, type: 'error' });
        })
    }

    getProjectData(projectId: string) {
        this.dashboardSerive.getDataProject(projectId).toPromise().then(Response => {
            if (Response.result) {
                this.selectedProject = this.projects.find(x => x.id == projectId);
                this.currentProjectData = Response.result;
            }
        }).catch(Response => {
            swal({ text: Response.error ? Response.error.text : Response.message, type: 'error' });
        })
    }

    projectSelectedChange(data: any) {
        var id = data.target.value;
        this.getProjectData(id);
    }

    createChart() {
        let data = new Data();
        let datasets = new Datasets();
        datasets.data.push(this.currentProjectData.imagesTagged);
        datasets.data.push(this.currentProjectData.totalImages - this.currentProjectData.imagesTagged);
        data.datasets.push(datasets);
        data.labels.push('Images tagged');
        data.labels.push('Images not tagged');

        this.imageTaggedChart = new Chart('canvas', {
            type: 'doughnut',
            data: data,
        });
        
        console.log(data);
    }
}
