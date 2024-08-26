import SubTitle from "./Styled/SubTitle.js";
import Button from "./Styled/Button.js";
import {useNavigate} from "react-router-dom";
import {useState, useContext} from "react";
import {UserNameContext} from "../App.jsx";
import Form from "./Styled/Form.js";
import Input from "./Styled/Input.js";


export default function Login(){
    const navigate = useNavigate();
    const { setUserName } = useContext(UserNameContext);

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [response, setResponse] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();

        const formData = {
            email,
            password,
        };

        try {
            const response = await fetch('/api/Auth/Login', {
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
            localStorage.setItem('authToken', data.token);
            localStorage.setItem('userName', data.userName);
            setUserName(data.userName);
            //console.log('Login successful:', data);
            navigate('/Main');
        } catch (error) {
            console.error('Error during login:', error);
        }
    };
    
    return (
        <div>
            <SubTitle>Login here:</SubTitle>
            <div className={"formCont"}>
            <Form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="email">Email:
                    <Input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    /></label>
                </div>
                <div>
                    <label htmlFor="password">Password:
                    <Input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    /></label>
                </div>
                <Button $primary type="submit">Login</Button>
            </Form>
            </div>
            <Button onClick={() => navigate('/')}>back</Button>
        </div>
    );
}