import './App.css'
import ParticlesBackground from "./Components/ParticlesBackground.jsx";
import {BrowserRouter, Route, Routes} from "react-router-dom";
import Home from "./Components/Home.jsx";
import Login from "./Components/Login.jsx";
import Registration from "./Components/Registration.jsx";
import UserMain from "./Components/UserMain.jsx";
import {createContext, useState} from "react";

export const UserNameContext = createContext({
    userName: '',
    setUserName: () => {}
});

function App() {
    const [userName, setUserName] = useState('');
    

  return (
    <UserNameContext.Provider value={{userName, setUserName}}>
        <ParticlesBackground />
        <BrowserRouter>
            <Routes>
                <Route path='/' element={<Home />} />
                <Route path='/Login' element={<Login />} />
                <Route path='/Register' element={<Registration />} />
                <Route path='/Main' element={<UserMain />} />
            </Routes>
        </BrowserRouter>
    </UserNameContext.Provider>
  )
}

export default App
