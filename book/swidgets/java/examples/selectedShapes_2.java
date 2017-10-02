import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;

public class selectedShapes_2 {
    public static void main(String[] args) {
        JFrame frame = new JFrame("add");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        SButton shape_1 = new SButton("Shape 1");
        SButton shape_2 = new SButton("Shape 1");
        SButton emptySpace = new SButton("Empty Space");

        Stream<String> shape1Selected = shape_1.sClicked.map(u -> "Shape 1 selected");
        Stream<String> shape2Selected = shape_2.sClicked.map(u -> "Shape 2 selected");
        Stream<String> emptySpaceSelected = emptySpace.sClicked.map(u -> "");

        Stream<String> unselectShape1 = emptySpaceSelected
                                .merge(shape2Selected, (s1, s2) -> "never happens")
                                .map(s -> "Shape 1 not selected");
        Stream<String> unselectShape2 = emptySpaceSelected
                                .merge(shape1Selected, (s1, s2) -> "never happens")
                                .map(s -> "Shape 2 not selected");

        Stream<String> shape1 = shape1Selected.merge(unselectShape1, (s1, s2) -> "never happens");
        Stream<String> shape2 = shape2Selected.merge(unselectShape2, (s1, s2) -> "never happens");


        SLabel lblSelection_1 = new SLabel(shape1.hold("Shape 1 not selected"));
        SLabel lblSelection_2 = new SLabel(shape2.hold("Shape 2 not selected"));

        frame.add(shape_1);
        frame.add(shape_2);
        frame.add(emptySpace);
        frame.add(lblSelection_1);
        frame.add(lblSelection_2);
        frame.setSize(400, 160);
        frame.setVisible(true);
    }
    private static Integer parseInt(String t) {
        try {
            return Integer.parseInt(t);
        } catch (NumberFormatException e) {
            return 0;
        }
    }
}
