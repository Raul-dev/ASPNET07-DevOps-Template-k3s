import React, { useState } from 'react';
import { AuthProvider, AuthProviderProps } from 'oidc-react';
import { api_url, oidc_client_secret, token_service_url } from './constants';
import Home from "./routes/Home"

const oidcConfig : AuthProviderProps = {
  authority: token_service_url,
  clientId: 'mainpage.client.spa',
  scope: "openid profile catalog.api.read",
  redirectUri: api_url + "/",
  clientSecret: oidc_client_secret,
  autoSignIn: false,
};

function App() {
  return (
    <AuthProvider {...oidcConfig}>
      <Home />
    </AuthProvider>    
  );
}
/*       
      
  */
export default App;