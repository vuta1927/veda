import { Component, OnInit } from '@angular/core';
import { ClassService } from '../../../services/class.service';
import { DataService } from '../../data.service';
import { QcOption, FilterOptions } from '../../../../shared/models/merge.model';
import { Export } from '../../../../shared/models/export.model';
import { ImportExportService } from '../../../services/import-export.service';
import { ProjectSettingService } from '../../../../+administrator/settings/settings.service';
import swal from 'sweetalert2';
@Component({
    selector: 'app-export-project',
    templateUrl : 'export-project.component.html',
    styleUrls: ['export-project.component.css']
})

export class ExportProjectComponent implements OnInit {
    messageHeader: string;
    message: string;
    currentProject: any = {};
    classSource: any = {};
    selectedClasses: any[] = [];
    qcFilterOptions: QcOption[] = [];
    qcLeves:any = Array(1).fill(1).map((x,i)=>i);
    constructor(
        private classService: ClassService,
        private dataService: DataService,
        private exportService: ImportExportService,
        private projectSettingService: ProjectSettingService
    ){
        const mother = this;
        this.dataService.currentProject.subscribe(p=>{
            mother.currentProject = p;
            mother.classService.getClasses(p.id).toPromise().then(response => {
                mother.classSource = response.result;
                mother.projectSettingService.getSetting(p.id).toPromise().then(Response=>{
                    if(Response.result){
                        // mother.qcLeves = Response.result.quantityCheckLevel;
                        // console.log(mother.qcLeves);
                    }
                }).catch(err=>{
                    if(err.error){
                        console.log(err.error.text);
                    }else{
                        console.log(err.message);
                    }
                });
            });
        });
        
    }

    ngOnInit(){

    }

    classSelectionChanged(data:any){
        this.selectedClasses = data.selectedRowsData;
    }

    classSelected(data){

    }

    qcLevelChange(e): void {
        const ele = e.target;
        const qcOpt = this.qcFilterOptions.find(x => x.index === ele.name);
        if (qcOpt) {
            qcOpt.value = ele.value;
        } else {
            this.qcFilterOptions.push({ index: ele.name, value: ele.value });
        }
    }

    Export(){
        const exportData = new Export();
        const mother = this;
        this.selectedClasses.forEach(c=>{
            exportData.classes.push(c.name);
         });
         
        exportData.filterOptions.qcOptions = this.qcFilterOptions;
        this.exportService.Export(this.currentProject.id, exportData).toPromise().then(Response=>{
            var blob = new Blob([Response], {type: "application/zip"});
            mother.saveAs(blob, mother.currentProject.name+".zip");
        }).catch(resp=>{
            swal({
                title: '',text: resp.error.message, type: 'error'
            })
        });
    }

    saveAs(blob, fileName) {
        var url = window.URL.createObjectURL(blob);
        var doc = document.createElement("a");
        doc.href = url;
        doc.download = fileName;
        doc.click();
        window.URL.revokeObjectURL(url);
    }
}