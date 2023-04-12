import { useAuth, withAuth, AuthContextProps } from "oidc-react";
import { useState } from "react";
import { api_url } from "../constants";
import './Home.css';


function formLink(path: string) : string {
    return api_url + "/" + path + "/";
}

const Home : any = () => {
  let [resultText, setResultText] = useState(api_url);

  const auth : AuthContextProps = useAuth();
  const accessToken = auth.userData?.access_token;

  let createButton = (apiPath: string, usesAuth: boolean = false) => {
    let path = formLink(apiPath);
    
    let onClick = async () => {
      let message = "no message";

      let fetchSettings : RequestInit = { method: "GET" }
      if(usesAuth === true && accessToken)
        fetchSettings.headers = {
          "Authorization": `Bearer ${accessToken}`
        }

      message = await fetch(path, fetchSettings).then(async (res) => {
        if(res.status >= 400 && res.status < 600) {
          return await res.status;
        }
        return await res.text();
      })
      .then((body) => {
        // HTTP 2.0 is the GOAT

        if(body === 401) return "Unauthorized"
        if(body === 404) return "Not Found"
        if(body === 500) return "Server Error"
         
        return body
      })
      .catch((err) => {
        return err
      })

      message = "Answer from server at location " + path + "\n" + message;

      setResultText(message);
      
    };

    return (<button onClick={onClick}> {apiPath} </button>)

  }

  
  let userInfo = auth.userData;
  let authButton = !!userInfo ? 
    <button onClick={auth.signOutRedirect}>logout</button> 
    : <button onClick={auth.signIn}>login</button>
  
  let UserInfoBlock = withAuth((props: AuthContextProps) => {
    return (
      <>
      <br />
      <h2 className="username">{props.userData?.profile.name}</h2>
      <br />
      <pre className="text-output">{JSON.stringify(props.userData, null, 2)}</pre>
      </>
    )
  })

    return (
        <div className="App wrapper">
      <div className="button-container" >
      {authButton}
      </div>
      <div className="button-container" >
        {createButton("api")}
        {createButton("identity")}
        {createButton("catalog", true)}
      </div>

      <pre className="text-output" id="text-output" >{resultText}</pre>
      <a href={formLink("ref")}>{formLink("ref")}</a>
      <br />
      <a href={formLink("admin")}>{formLink("admin")}</a>
      {auth.userData ? <UserInfoBlock /> : <></>}
    </div>
    )
}

export default Home;