import { Component, OnInit, OnDestroy, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef } from "@angular/core";
import { ActivatedRoute, Router, RouterStateSnapshot } from "@angular/router";
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../helpers';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../../shared/services/security.service';
import { Constants } from '../../constants';
import { ClassService } from '../services/class.service';

@Component({
    selector: 'app-merge-project',
    styleUrls: ['merge-project.component.css'],
    templateUrl: 'merge-project.component.html',
    encapsulation: ViewEncapsulation.None
})

export class MergeProject implements OnInit {
    @ViewChildren(DxDataGridComponent) dataGrid: DxDataGridComponent
    selectedUsers: any[] = [];
    selectedClasses: any[] = [];
    classSource: any = {};

    constructor(
        private route: Router,
        private activatedRoute: ActivatedRoute,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private classService: ClassService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        let mother = this;
        activatedRoute.params.subscribe(params => {
            console.log(params.projects.split(',')); 
            mother.classSource.store = new CustomStore({
                load: function (loadOptions: any) {
                    return mother.classService.getClassProjects(params.projects)
                        .toPromise().then(response => {
                            return {
                                data: response.result,
                                totalCount: response.result.length,
                            }
                        }).catch(error => { throw 'Data Loading Error' });
                }
            });
        });
    }

    ngOnInit() {

    }

    ngOnDestroy(): void {
        //Called once, before the instance is destroyed.
        //Add 'implements OnDestroy' to the class.
    }

    classSelectionChanged(data:any){
        this.selectedClasses = data.selectedRowsData;

    }
}