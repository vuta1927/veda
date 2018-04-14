export interface IAppCoreResponse<T> {
    result: T;
    targetUrl: string;
    success: boolean;
    error: any;
    unAuthorizedRequest: boolean;
    __app: boolean;
}