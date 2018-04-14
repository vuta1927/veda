import { Component, ViewEncapsulation, ViewChild } from '@angular/core';
import { UsersService } from './users.service';
import CustomStore from 'devextreme/data/custom_store';

import 'rxjs/add/operator/toPromise';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { DxDataGridComponent } from 'devextreme-angular';
import { CreateOrUpdateUserComponent } from './create-or-update-user.component';
import { TranslateExtService } from '../../shared/services/translate-ext.service';
import { Helpers } from '../../helpers';

@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: [
        './users.component.scss'
    ],
    encapsulation: ViewEncapsulation.None
})
export class UsersComponent {

    menuItems = [
        {
            icon: 'preferences',
            items: [
                { text: this.translate.get('Edit'), icon: 'la la-edit', value: 1 },
                { text: this.translate.get('Delete'), icon: 'la la-trash', value: 2 }
            ]
        }
    ]

    dataSource: any = {};
    @ViewChild(DxDataGridComponent) dataGrid: DxDataGridComponent;

    constructor(private usersService: UsersService, private modalService: NgbModal, private translate: TranslateExtService) {

        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                var params = '';

                params += loadOptions.skip || 0;
                params += '/' + loadOptions.take || 12;

                // if (loadOptions.sort) {
                //     params += '&orderBy=' + loadOptions.sort[0].selector;
                //     if (loadOptions.sort[0].desc) {
                //         params += ' desc';
                //     }
                // }

                return usersService.getUsers(params)
                    .toPromise()
                    .then(response => {
                        return {
                            data: response.result,
                            totalCount: response.result.length
                        };
                    })
                    .catch(error => { throw 'Data loading error' });
            }
        });
    }

    exportToExcel() {
        this.dataGrid.instance.exportToExcel(false);
    }

    openCreateOrUpdateModal(id?: number) {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        Helpers.setLoading(true);
        let mother = this;
        this.usersService.getUserForCreatOrEdit(id).subscribe(res => {
            console.log(res.result);
            const modalRef = this.modalService.open(CreateOrUpdateUserComponent, config);
            modalRef.componentInstance.userForCreateOrEdit = res.result;
            modalRef.result.then(function () {
                mother.refreshGrids();
            })
            Helpers.setLoading(false);
        }, _ => {
            Helpers.setLoading(false);
        });
    }
    
    refreshGrids() {
        this.dataGrid.instance.refresh();
    }

    itemClick($event, data) {
        console.log($event, data);
            this.openCreateOrUpdateModal(data.data.id);
    }
}