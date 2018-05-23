import { Component, ViewEncapsulation, ViewChild, ViewChildren, OnInit } from '@angular/core';
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
import { Constants } from '../../constants';
@Component({
    selector: 'app-analytics',
    templateUrl: './analytics.component.html',
    styleUrls: ['./analytics.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class AnalyticsComponent implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    imageTaggedChart: any = [];
    TagChart: any = [];
    QcChart : any = [];
    projects = [];
    userDataSource = {};
    selectedProject: any;
    currentProjectData = new projectAnalist();
    percentData: Data = new Data();
    isProjectManager: boolean = false;
    isQc: boolean = false;
    isTeacher: boolean = false;
    constructor(
        private dashboardSerive: DashBoardService,
        private securityService: SecurityService
    ) { }

    ngOnInit(){
        this.isProjectManager = this.securityService.isInRole(Constants.ProjectManager);
        this.isQc = this.securityService.isInRole(Constants.QuantityCheck);
        this.isTeacher = this.securityService.isInRole(Constants.Teacher);
        
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
        Helpers.setLoading(true);
        this.dashboardSerive.getDataProject(projectId).toPromise().then(Response => {
            if (Response && Response.result) {
                this.selectedProject = this.projects.find(x => x.id == projectId);
                this.currentProjectData = Response.result;
                this.userDataSource = Response.result.userProjects;
                this.customChart();
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
        this.userDataSource = {};
        this.TagChart.destroy();
        this.QcChart.destroy();
        this.imageTaggedChart.destroy();
        this.currentProjectData = new projectAnalist();
        this.percentData = new Data();
        this.resetPorgressbar();
        this.getProjectData(id);
    }

    resetPorgressbar(){
        let qcPrgressBar = document.getElementById('totalQcs-pg');
        let imagesHaveTagBar = document.getElementById('imagesHaveTag-pg');
        let tagsHaveClassBar = document.getElementById('tagsHaveClass-pg');
        
        qcPrgressBar.style.width = imagesHaveTagBar.style.width = tagsHaveClassBar.style.width = '0%';
    }

    customChart() {
        Chart.pluginService.register({
            beforeDraw: function (chart) {
                if (chart.config.options.elements.center) {
                    //Get ctx from string
                    var ctx = chart.chart.ctx;

                    //Get options from the center object in options
                    var centerConfig = chart.config.options.elements.center;
                    var fontStyle = centerConfig.fontStyle || 'Arial';
                    var txt = centerConfig.text;
                    var color = centerConfig.color || '#000';
                    var sidePadding = centerConfig.sidePadding || 20;
                    var sidePaddingCalculated = (sidePadding / 100) * (chart.innerRadius * 2)
                    //Start with a base font of 30px
                    ctx.font = "30px " + fontStyle;

                    //Get the width of the string and also the width of the element minus 10 to give it 5px side padding
                    var stringWidth = ctx.measureText(txt).width;
                    var elementWidth = (chart.innerRadius * 2) - sidePaddingCalculated;

                    // Find out how much the font can grow in width.
                    var widthRatio = elementWidth / stringWidth;
                    var newFontSize = Math.floor(30 * widthRatio);
                    var elementHeight = (chart.innerRadius * 2);

                    // Pick a new font size so it will not be larger than the height of label.
                    var fontSizeToUse = Math.min(newFontSize, elementHeight);

                    //Set font settings to draw it correctly.
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    var centerX = ((chart.chartArea.left + chart.chartArea.right) / 2);
                    var centerY = ((chart.chartArea.top + chart.chartArea.bottom) / 2);
                    ctx.font = '30px ' + fontStyle;
                    // ctx.font = fontSizeToUse + "px " + fontStyle;
                    ctx.fillStyle = color;

                    //Draw text in center
                    ctx.fillText(txt, centerX, centerY);
                }
            }
        });
    }

    createImageChart() {
        const mother = this;
        this.percentData.imageTaggedPercent = mother.currentProjectData.totalImages==0? 0: Math.round((mother.currentProjectData.imagesTagged / mother.currentProjectData.totalImages) * 100);
        this.percentData.imageNotTaggedPercent = mother.currentProjectData.totalImages==0? 0: Math.round(((mother.currentProjectData.totalImages - mother.currentProjectData.imagesTagged) / mother.currentProjectData.totalImages) * 100);
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
                },
                elements: {
                    center: {
                        text: mother.currentProjectData.totalImages,
                        color: '#FF6384', // Default is #000000
                        fontStyle: 'Arial', // Default is Arial
                        sidePadding: 20 // Defualt is 20 (as a percentage)
                    }
                }
            }
        });


    }

    createTagChart() {
        const mother = this;
        this.percentData.tagsHaveClass = mother.currentProjectData.totalTags==0? 0: Math.round((mother.currentProjectData.totalTagsHaveClass / mother.currentProjectData.totalTags) * 100);
        this.percentData.tagsNotHaveClass = mother.currentProjectData.totalTags==0? 0: Math.round(((mother.currentProjectData.totalTags - mother.currentProjectData.totalTagsHaveClass) / mother.currentProjectData.totalTags) * 100);
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
                },
                elements: {
                    center: {
                        text: mother.currentProjectData.totalTags,
                        color: '#34BFA3', // Default is #000000
                        fontStyle: 'Arial', // Default is Arial
                        sidePadding: 20 // Defualt is 20 (as a percentage)
                    }
                }
            }
        })
    }

    createQcChart() {
        const mother = this;
        this.percentData.imagesHaveQcPercent = mother.currentProjectData.imagesTagged==0? 0: Math.round((mother.currentProjectData.imagesHadQc / mother.currentProjectData.totalImages) * 100);
        this.percentData.imagesNotHaveQcPercent = mother.currentProjectData.imagesTagged==0? 0: Math.round(((mother.currentProjectData.imagesTagged - mother.currentProjectData.imagesHadQc) / mother.currentProjectData.totalImages) * 100);
        let data = {
            datasets: [{
                data: [mother.currentProjectData.imagesHadQc, (mother.currentProjectData.totalImages - mother.currentProjectData.imagesHadQc)],
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
                },
                elements: {
                    center: {
                        text: mother.currentProjectData.totalImages,
                        color: '#716ACA', // Default is #000000
                        fontStyle: 'Arial', // Default is Arial
                        sidePadding: 20 // Defualt is 20 (as a percentage)
                    }
                }
            }
        })
    }
}
