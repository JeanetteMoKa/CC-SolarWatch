import styled from "styled-components";

const SolarDataGrid = styled.div`
  /* Adapt the colors based on primary prop */
        background: #212d6a;
        color: thistle;
        display: grid;
        grid-template-columns: 24% 24% 24% 24%;
        grid-column-gap: 1%;
        font-size: 1em;
        padding: 4px 0;
        border-radius: 10px;
        width: 372px;
    
    & p {
        margin: 0;
    }
`;
export default SolarDataGrid;