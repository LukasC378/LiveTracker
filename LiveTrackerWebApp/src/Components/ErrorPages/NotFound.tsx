import {Link} from "react-router-dom";

const NotFoundComponent = () => {
    return (
        <div className={"notFound p-4"}>
            <h1>404 Not Found</h1>
            <Link to={"/"}>Home</Link>
        </div>
    );
}

export default NotFoundComponent;