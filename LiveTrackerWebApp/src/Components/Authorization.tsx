import React, {useContext, useEffect, useState} from "react";
import UserService from "../Services/UserService";
import {Constants} from "../Constants";
import {UserContext} from "../App";
import LoadingComponent from "./Loading";
import {useNavigate} from "react-router-dom";
import {UserRoleEnum} from "../Models/Enums";

type AuthorizationComponentProps = {
    component: React.ComponentType<any>;
    userRole?: UserRoleEnum;
};
const AuthorizationComponent: React.FC<AuthorizationComponentProps> = ({ component: Component, userRole: role }) => {

    const navigate = useNavigate();
    let {setCurrentUser}: any = useContext(UserContext);

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);

    useEffect(() => {
        UserService.getCurrentUserWithAuthorization().then((res) => {
            if(res.data == null){
                setCurrentUser(() => Constants.defaultUser)
                navigate("/login")
                return
            }
            if(role !== undefined && role !== res.data!.role){
                navigate("/forbidden")
                return
            }
            setCurrentUser(() => res.data);
            setIsLoading(() => false);
        }).catch(() => {
            setCurrentUser(() => Constants.defaultUser);
            navigate("/login")
        })
    }, []);

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <div>
            <Component />
        </div>
    );
};

export default AuthorizationComponent;