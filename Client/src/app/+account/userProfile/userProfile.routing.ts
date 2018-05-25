import { Routes } from '@angular/router';
import { UserProfileComponent } from './userProfile.component';
import { DefaultComponent } from '../../shared/components/pages/default/default.component';

export const routes: Routes = [
    {
        path: '',
        component: DefaultComponent,
        children:[
            {
                path:'',
                component: UserProfileComponent
            }
        ]
    }
];