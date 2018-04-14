import { environment } from '../environments/environment';
export class Constants {
    /** Date format */
    public static DATE_FMT = 'dd/MM/yyyy';
    public static DATE_FMT_JQUI = 'dd/mm/yy';
    public static DATE_TIME_FMT = `${Constants.DATE_FMT} hh:mm:ss`;

    /** API */
    public static REGISTER = '/register';
    public static LOGIN = '/login';

    public static LOG_OUT = `${environment}/logout`;

    public static USER_INFO = 'assets/api/user/login-info.json';
    public static ACTIVITIES = 'assets/api/activities/activities.json';


    //Claim
    public static admin = 'Administrator';

    public static editProject = 'EditProject';
    public static addProject = 'AddProject';
    public static deleteProject = 'DeleteProject';
    public static viewProject = 'ViewProject';

    public static editImage = 'EditImage';
    public static addImage = 'AddImage';
    public static deleteImage = 'DeleteImage';
    public static viewImage = 'ViewImage';

    public static QcEdit = 'EditQuantityCheck';
    public static QcAdd = 'AddQuantityCheck';
    public static QcDelete = 'DeleteQuantityCheck';
    public static QcView = 'ViewQuantityCheck';

    public static EditUser = 'EditUser';
    public static AddUser = 'AddUser';
    public static DeleteUser = 'DeleteUser';
    public static ViewUser = 'ViewUser';

    public static ViewRole = 'ViewRole';
    public static EditRole = 'EditRole';
    public static AddRole = 'AddRole';
    public static DeleteRole = 'DeleteRole';
}