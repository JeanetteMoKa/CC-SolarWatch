import Title from "./Styled/Title.js";
import SubTitle from "./Styled/SubTitle.js";
import Button from "./Styled/Button.js";
import Form from "./Styled/Form.js";
import Input from "./Styled/Input.js";
import {useContext, useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import {UserNameContext} from "../App.jsx";
import PlaceName from "./Styled/PlaceName.js";
import SolarDataGrid from "./Styled/SolarDataGrid.js";

const formatTime = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'});
};

export default function UserMain() {
    const navigate = useNavigate();
    const {userName, setUserName} = useContext(UserNameContext);

    const [cityName, setCityName] = useState('');
    const [countryName, setCountryName] = useState('');
    const [stateName, setStateName] = useState('');
    const [date, setDate] = useState('');
    const [response, setResponse] = useState(null);

    const [sunrise, setSunrise] = useState('');
    const [sunset, setSunset] = useState('');


    useEffect(() => {
        if (!userName) {
            navigate('/Login');
        }
    }, [userName, navigate]);

    const handleLogout = (whereTo) => {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userName');
        setUserName('');
        navigate(whereTo);
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        const token = localStorage.getItem('authToken');


        const url = new URL(`/api/SolarData/${encodeURIComponent(cityName)}/${encodeURIComponent(date)}`, window.location.origin);
        url.searchParams.append('countryName', countryName);
        if (countryName.toLowerCase() === 'usa' || countryName.toLowerCase() === 'united states of america') {
            url.searchParams.append('stateName', stateName);
        }

        try {
            const responseData = await fetch(url, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`, // Include the token here
                },
            });

            if (!responseData.ok) {
                throw new Error('Failed to fetch data');
            }

            const data = await responseData.json();
            setResponse(data);
            console.log('Protected data:', data);
            setSunset(() => formatTime(data.sunset));
            setSunrise(() => formatTime(data.sunrise));
        } catch (error) {
            console.error('Error fetching protected data:', error);
        }
    };
    const handleReset = () => {
        setCityName('');
        setCountryName('');
        setStateName('');
        setDate('');
        setResponse(null);
        setSunrise('');
        setSunset('');
    }

    useEffect(() => {
        if (response) {
            console.log(sunrise + ' and ' + sunset);
        }

    }, [response]);


    return (<>
        <Title>Dusk and Dawn</Title>
        <SubTitle>Welcome again {userName}, enter the required data to see the sunset and sunrise times.</SubTitle>
        {sunset === '' && sunrise === '' ?
        (<div className={"formCont"}>
            <Form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="city">City:
                        <Input
                            type="text"
                            id="city"
                            value={cityName}
                            onChange={(e) => setCityName(e.target.value)}
                            required
                        />
                    </label>
                </div>
                <div>
                    <label htmlFor="country">Country:
                        <Input
                            type="text"
                            id="country"
                            value={countryName}
                            onChange={(e) => setCountryName(e.target.value)}
                            required
                        />
                    </label>
                </div>
                {countryName.trim().toLowerCase() === 'united states of america' && (<div>
                    <label htmlFor="state">State:
                        <Input
                            type="text"
                            id="state"
                            value={stateName}
                            onChange={(e) => setStateName(e.target.value)}
                            required
                        />
                    </label>
                </div>)}
                <div>
                    <label htmlFor="date">Date:
                        <Input
                            type="date"
                            id="date"
                            value={date}
                            onChange={(e) => setDate(e.target.value)}
                            required
                        />
                    </label>
                </div>
                <Button $primary type="submit">Submit</Button>
            </Form>
        </div>)
        : ( <>
                <div className={"dataDisplay"}>
                    <PlaceName>{cityName}, {countryName}
                    {stateName ? ` (${stateName})` : ''}
                    <br/>
                    {new Date(date).toLocaleDateString()}
                    </PlaceName>
                    <SolarDataGrid>
                        <p>Sunrise: </p>
                        <p>{sunrise}</p>
                        <p>Sunset: </p>
                        <p>{sunset}</p>
                    </SolarDataGrid>
                </div>
                <Button $primary onClick={() => handleReset()}>Reset</Button>
            </>)}
        <Button onClick={() => handleLogout('/')}>logout</Button>
    </>);
}