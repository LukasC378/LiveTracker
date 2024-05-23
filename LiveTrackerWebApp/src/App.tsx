import {BrowserRouter, Navigate, Route, Routes} from "react-router-dom";
import './App.css'
import HomePage from "./Components/HomePage/HomePage";
import NavigationComponent from "./Components/Navigation/Navigation";
import NotFound from "./Components/ErrorPages/NotFound";
import React, {createContext, useEffect, useState} from "react";
import {Constants} from "./Constants";
import {ErrorBoundary} from "react-error-boundary";
import {User} from "./Models/User";
import MyCollections from "./Components/Collections/MyCollections";
import Authorization from "./Components/Authorization";
import Recaptcha from "./Components/Recaptcha";
import RegistrationFirst from "./Components/Registration/RegistrationFirst";
import RegistrationSecond from "./Components/Registration/RegistrationSecond";
import Login from "./Components/Login/Login";
import LiveSessions from "./Components/Sessions/LiveSessions";
import SessionsSchedule from "./Components/Sessions/SessionsSchedule";
import UserService from "./Services/UserService";
import CollectionEditor from "./Components/Collections/CollectionEditor";
import ErrorPage from "./Components/ErrorPages/ErrorPage";
import Organizers from "./Components/Users/Organizers";
import ForbiddenPage from "./Components/ErrorPages/ForbiddenPage";
import {UserRoleEnum} from "./Models/Enums";
import Race from "./Components/Race/Race";
import MyLayouts from "./Components/Layouts/MyLayouts";
import LayoutEditor from "./Components/Layouts/LayoutEditor";
import SessionEditor from "./Components/Sessions/SessionEditor";
import SessionsManager from "./Components/Sessions/SessionsManager";
import RaceResult from "./Components/Sessions/SessionResult";

export let UserContext: any = createContext([Constants.defaultUser, () => {return;}]);

function App() {

    const [currentUser, setCurrentUser]: [User, any] = useState(Constants.defaultUser);
    const userState = {currentUser, setCurrentUser};

    useEffect(() => {
        const storedUser = sessionStorage.getItem('user');

        if(storedUser){
            setCurrentUser(() => JSON.parse(storedUser));
            return;
        }
        UserService.getCurrentUser().then((res) => {
            let user: User;
            if(res.data)
                user = res.data;
            else
                user = Constants.defaultUser;
            sessionStorage.setItem('user', JSON.stringify(user));
            setCurrentUser(() => user);
        }).catch(() => {
            setCurrentUser(() => Constants.defaultUser);
        })
    }, []);

    return (
        <UserContext.Provider value={userState}>
                <BrowserRouter>
                    <NavigationComponent/>
                    <Routes>
                        <Route path="/" element={<HomePage/>} />

                        <Route path={"/login"} element={<Recaptcha component={Login}/>} />
                        <Route path={"/sign-up"} element={<Recaptcha component={RegistrationFirst}/>} />
                        <Route path={"/sign-up/:registrationLink"} element={<Recaptcha component={RegistrationSecond}/>} />

                        <Route path={"/collections"} element={<Authorization component={MyCollections} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/collections/edit/:id"} element={<Authorization component={CollectionEditor} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/collections/create"} element={<Authorization component={CollectionEditor} userRole={UserRoleEnum.Organizer}/>} />

                        <Route path={"/layouts"} element={<Authorization component={MyLayouts} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/layouts/edit/:id"} element={<Authorization component={LayoutEditor} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/layouts/create"} element={<Authorization component={LayoutEditor} userRole={UserRoleEnum.Organizer}/>} />

                        <Route path={"/sessions/create"} element={<Authorization component={SessionEditor} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/sessions/manage"} element={<Authorization component={SessionsManager} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/sessions/edit/:id"} element={<Authorization component={SessionEditor} userRole={UserRoleEnum.Organizer}/>} />
                        <Route path={"/sessions/live"} element={<LiveSessions/>} />
                        <Route path={"/sessions/schedule"} element={<SessionsSchedule/>} />
                        <Route path={"/sessions/:sessionId"} element={<Race/>} />
                        <Route path={"/sessions/result/:sessionId"} element={<RaceResult/>} />

                        <Route path={"/organizers"} element={<Authorization component={Organizers} userRole={UserRoleEnum.NormalUser}/>} />

                        <Route path="/404" element={<NotFound/>}/>
                        <Route path="/error" element={<ErrorPage/>}/>
                        <Route path="/forbidden" element={<ForbiddenPage/>}/>
                        <Route path={"/*"} element={<Navigate to='/404' />}/>
                    </Routes>
                </BrowserRouter>
        </UserContext.Provider>
    )
}

export default App
