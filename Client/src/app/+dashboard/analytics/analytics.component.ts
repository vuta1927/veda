import { Component, ViewEncapsulation, ViewChild, ViewChildren } from '@angular/core';
import { SecurityService } from '../../shared/services/security.service';
import { OAuthService } from 'angular-oauth2-oidc';
import { Chart } from 'chart.js';
import { DashBoardService } from './analytics.service';
import swal from 'sweetalert2';
import responsive_box from 'devextreme/ui/responsive_box';
import { projectAnalist } from '../../shared/models/dashboard.model';
import { Data } from '../../shared/models/chart.model';
import { Helpers } from '../../helpers';
import { DxDataGridComponent } from 'devextreme-angular';
@Component({
    selector: 'app-analytics',
    templateUrl: './analytics.component.html',
    styleUrls: ['./analytics.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class AnalyticsComponent {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    imageTaggedChart = [];
    TagChart = [];
    QcChart = [];
    projects = [];
    userDataSource = {};
    selectedProject: any;
    currentProjectData = new projectAnalist();
    percentData: Data = new Data();
    constructor(
        private dashboardSerive: DashBoardService
    ) {
        Helpers.setLoading(true);
        this.dashboardSerive.getProjectsByUser().toPromise().then(Response => {
            if (Response && Response.result) {
                this.projects = Response.result;
                this.selectedProject = Response.result[0];
                this.getProjectData(Response.result[0].id);
            }
        }).catch(Response => {
            Helpers.setLoading(false);
            swal({ text: Response.error ? Response.error.text : Response.message, type: 'error' });
        })
    }

    getProjectData(projectId: string) {
        this.dashboardSerive.getDataProject(projectId).toPromise().then(Response => {
            if (Response && Response.result) {
                this.selectedProject = this.projects.find(x => x.id == projectId);
                this.currentProjectData = Response.result;
                this.userDataSource = Response.result.userProjects;
                this.createImageChart();
                this.createTagChart();
                this.createQcChart();
                Helpers.setLoading(false);
            }
        }).catch(Response => {
            Helpers.setLoading(false);
            swal({ text: Response.error ? Response.error.text : Response.message, type: 'error' });
        })
    }

    projectSelectedChange(data: any) {
        var id = data.target.value;
        this.getProjectData(id);
    }

    createImageChart() {
        const mother = this;
        this.percentData.imageTaggedPercent = Math.round((mother.currentProjectData.imagesTagged / mother.currentProjectData.totalImages) * 100);
        this.percentData.imageNotTaggedPercent = Math.round(((mother.currentProjectData.totalImages - mother.currentProjectData.imagesTagged) / mother.currentProjectData.totalImages) * 100);
        let data = {
            datasets: [{
                data: [mother.currentProjectData.imagesTagged, (mother.currentProjectData.totalImages - mother.currentProjectData.imagesTagged)],
                backgroundColor: [
                    "#FF6384"
                ],
                hoverBackgroundColor: [
                    "#FF6384"
                ]
            }],

            // These labels appear in the legend and in the tooltips when hovering different arcs
            labels: [
                'Images Tagged',
                'Images not Tagged'
            ]
        };
        this.imageTaggedChart = new Chart('canvas', {
            type: 'doughnut',
            data: data,
            options: {
                legend: {
                    display: false
                }
            }
        });
    }

    createTagChart() {
        const mother = this;
        this.percentData.tagsHaveClass = Math.round((mother.currentProjectData.totalTagsHaveClass / mother.currentProjectData.totalTags) * 100);
        this.percentData.tagsNotHaveClass = Math.round(((mother.currentProjectData.totalTags - mother.currentProjectData.totalTagsHaveClass) / mother.currentProjectData.totalTags) * 100);
        let data = {
            datasets: [{
                data: [mother.currentProjectData.totalTagsHaveClass, (mother.currentProjectData.totalTags - mother.currentProjectData.totalTagsHaveClass)],
                backgroundColor: [
                    "#34BFA3"
                ],
                hoverBackgroundColor: [
                    "#34BFA3"
                ]
            }],

            // These labels appear in the legend and in the tooltips when hovering different arcs
            labels: [
                'Tags have class',
                'Tags not have class'
            ]
        };
        this.TagChart = new Chart('canvas-tag', {
            type: 'doughnut',
            data: data,
            options: {
                legend: {
                    display: false
                }
            }
        })
    }

    createQcChart(){
        const mother = this;
        this.percentData.imagesHaveQcPercent = Math.round((mother.currentProjectData.imagesHadQc / mother.currentProjectData.imagesTagged) * 100);
        this.percentData.imagesNotHaveQcPercent = Math.round(((mother.currentProjectData.imagesTagged - mother.currentProjectData.imagesHadQc) / mother.currentProjectData.imagesTagged) * 100);
        let data = {
            datasets: [{
                data: [mother.currentProjectData.imagesHadQc, (mother.currentProjectData.imagesTagged - mother.currentProjectData.imagesHadQc)],
                backgroundColor: [
                    "#716ACA"
                ],
                hoverBackgroundColor: [
                    "#716ACA"
                ]
            }],

            // These labels appear in the legend and in the tooltips when hovering different arcs
            labels: [
                'Images have Qc',
                'Images not have Qc'
            ]
        };
        this.QcChart = new Chart('canvas-qc', {
            type: 'doughnut',
            data: data,
            options: {
                legend: {
                    display: false
                }
            }
        })
    }
}
