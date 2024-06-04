import {Col, Image, Row} from "react-bootstrap";
import liveRaceImage from "../../assets/live-race.png";
import collectionEditorImage from "../../assets/collection-editor.png";

const HomeComponent = () => {
    return(
      <div className={"container-fluid"}>
          <Row className={'mt-2'}>
              <h1>WELCOME TO LIVE TRACKER</h1>
          </Row>
          <Row className={'mt-5'}>
              <Col>
                  <Image src={liveRaceImage} width={700}/>
              </Col>
              <Col>
                  <Image src={collectionEditorImage} width={700}/>
              </Col>
          </Row>
          <Row className={'mt-5'}>
              <div style={{fontSize: 20}}>
                  This web application provides organizers a comprehensive interface for event creation and management of driver and team collections.<br/>
                  Viewers can watch races online or access archived content.<br/>
                  Throughout the race, users can customize their view using the map display.<br/>
                  A key aspect is the algorithm for calculating driver positions based solely on received GPS data from the organizer.
              </div>
          </Row>
      </div>
    );
}

export default HomeComponent;