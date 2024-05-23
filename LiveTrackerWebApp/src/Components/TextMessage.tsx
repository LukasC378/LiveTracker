import React from "react";

const TextMessageComponent = (props: any) => {
    return(
        <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
            <div className={"p-4"}>
                <p>{props.message}</p>
            </div>
        </div>
    )
}

export default TextMessageComponent