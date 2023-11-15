import { Icon } from "@mui/material";
import RealDeaLsLogo from "../../assets/Logo.svg"
import React from "react";

export const AppLogo = () => (
    <Icon data-testid="app-logo-icon" style={{height: 100, width: 100}}>
        <img data-testid="app-logo-image" src={RealDeaLsLogo} height={100} width={100}/>
    </Icon>
)