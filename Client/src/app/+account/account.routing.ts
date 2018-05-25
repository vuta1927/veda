import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';
import { DefaultComponent } from '../shared/components/pages/default/default.component';
import { AuthGuard } from '../shared/guards/auth.guard';
export const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'logout',
        component: LogoutComponent
    },
    
]