import styled from "styled-components";

const Button = styled.button`
  /* Adapt the colors based on primary prop */
        background: ${props => (props.$primary ? '#212d6a' : 'thistle')};
        color: ${props => (props.$primary ? 'thistle' : '#212d6a')};
        font-size: 1em;
        margin: 1em;
        padding: 0.4em 1em;
        border: 3px solid #212d6a;
        border-radius: 10px;
        width: 150px;
        transition: all 0.5s ease;

        &:hover {
            background: ${props => (props.$primary ? 'thistle' : '#212d6a')};
            color: ${props => (props.$primary ? '#212d6a' : 'thistle')};
            border-color: ${props => (props.$primary ? '#212d6a' : 'thistle')};
        }
`;
export default Button;