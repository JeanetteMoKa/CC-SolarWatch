import SubTitle from "./Styled/SubTitle.js";
import Button from "./Styled/Button.js";
import {useNavigate} from "react-router-dom";
import {useState} from "react";
import Form from "./Styled/Form.js";
import Input from "./Styled/Input.js";


export default function Registration(){
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        email: '',
        username: '',
        password: '',
    });
    const [response, setResponse] = useState(null);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value,
        });
    };
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await fetch('/api/Auth/Register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData),
            });

            if (!response.ok) {
                const errorData = await response.json();
                alert(errorData[Object.keys(errorData)]);
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            setResponse(data);
            navigate('/Login');
        } catch (error) {
            console.error('There was a problem with the fetch operation:', error);
        }
    };

    
    
    return(
        <div>
            <SubTitle>Sign up here:</SubTitle>
            <div  className={"formCont"}>

            <Form onSubmit={handleSubmit}>
                <div>
                    <label>Email:
                    <Input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={handleChange}
                        required
                    /></label>
                </div>
                <div>
                    <label>Username:
                    <Input
                        type="text"
                        name="username"
                        value={formData.username}
                        onChange={handleChange}
                        required
                    /></label>
                </div>
                <div>
                    <label>Password:
                    <Input
                        type="password"
                        name="password"
                        value={formData.password}                        
                        onChange={handleChange}
                        required
                    /></label>
                </div>
                <Button type={"submit"}>Register</Button>
            </Form></div>
            <div className={"buttonCont"}>
                <Button onClick={() => navigate('/')}>back</Button>
                <Button onClick={() => navigate('/Login')}>login</Button>
            </div>
        </div>
    );
}