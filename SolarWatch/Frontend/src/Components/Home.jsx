import Title from "./Styled/Title.js";
import SubTitle from "./Styled/SubTitle.js";
import Button from "./Styled/Button.js";
import {useNavigate} from "react-router-dom";


export default function Home(){
    const navigate = useNavigate();
    
    return(<>
        <Title>Dawn and Dusk</Title>
        <SubTitle>Your reliable sunset and sunrise times viewer</SubTitle>
        <div className={"buttonCont"}>
            <Button onClick={() => navigate('/Login')}>login</Button>
            <Button onClick={() => navigate('/Register')}>sign up</Button>
        </div>
    </>)
}