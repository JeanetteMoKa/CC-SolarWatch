import styled from "styled-components";

const Form = styled.form`
    width: 100%;
    max-width: 300px; /* Adjust max-width as needed */
    margin: 0 auto; /* Center the form horizontally */
    padding: 36px 36px 20px 36px;
    background-color: #FFFFFF3C;
    border-color: transparent;
    border-radius: 20px;
    font-weight: bolder;

    & label {
        display: flex;
        flex-direction: row;
        align-items: flex-start;
        padding-left: 10px;
        position: relative;
    }
`
export default Form;

