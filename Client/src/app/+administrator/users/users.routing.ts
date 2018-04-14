import { Routes } from '@angular/router';
import { UsersComponent } from './users.component';
import { DefaultComponent } from '../../shared/components/pages/default/default.component';

export const routes: Routes = [
    {
        path: '',
        component: DefaultComponent,
        children:[
            {
                path:'',
                component: UsersComponent
            }
        ]
    }
];