import { AuthConfig } from 'angular-oauth2-oidc';

export function authPasswordFlowConfig(identityUrl: string): AuthConfig {

    return {
        // Url of the Identity Provider
        issuer: identityUrl,

        // URL of the SPA to redirect the user to after login
        redirectUri: window.location.origin,

        // URL of the SPA to redirect the user after silent refresh
        silentRefreshRedirectUri: window.location.origin,

        clientId: 'vds-client',

        dummyClientSecret: '40A00C685411260BD89DF2459D8EE35FDE2FFAA3AD103EA9CD4362B544CEFE63',

        scope: 'openid profile offline_access vds-api',

        showDebugInformation: true,

        oidc: false,

        requireHttps: false
    }
}