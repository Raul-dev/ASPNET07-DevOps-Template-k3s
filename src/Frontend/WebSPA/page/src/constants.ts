export const api_host = process.env.REACT_APP_API_URL || window.location.host;
export const api_url = `${window.location.protocol}//${api_host}`

export const token_service_host = process.env.REACT_APP_TOKEN_SERVICE_URL || window.location.host +  "/identity";
export const token_service_url = `${window.location.protocol}//${token_service_host}`

export const oidc_client_secret = process.env.REACT_APP_OIDC_CLIENT_SECRET;

